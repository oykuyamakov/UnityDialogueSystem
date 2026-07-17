#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector.Editor;
#endif
using UnityEditor;
using UnityEngine;

namespace Cadi.Scripts.CacherSystem.Editor
{
    internal sealed class CachedFieldInfo
    {
        public FieldInfo Field;
        public CachedFieldAttribute Attr;
    }

    internal static class CacherEditorHelper
    {
        private static readonly Dictionary<Type, List<CachedFieldInfo>> s_CachedFieldsByType = new();
        private static readonly Dictionary<Type, List<string>> s_WarningCacheByType = new();

        internal static readonly GUIContent s_CachedHeader = new("Cached References");
        internal static readonly GUIContent s_ResolveNow = new("Resolve Now");
        internal static readonly GUIContent s_ShowMeta = new("Show meta");

        internal static void DrawCachedSection(
            UnityEditor.Editor editor,
            ref bool showMeta)
        {
            var t0 = editor.target as CacherMonoBehaviour;
            if (t0 == null)
                return;

            Type type = t0.GetType();
            List<CachedFieldInfo> cached = GetCachedFields(type);

            if (cached.Count == 0)
                return;

            string foldoutKey = $"CacherMonoBehaviourEditor_Foldout_{type.FullName}";
            bool expanded = SessionState.GetBool(foldoutKey, true);

            using (new EditorGUILayout.VerticalScope(EditorStyles.helpBox))
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    expanded = EditorGUILayout.Foldout(expanded, s_CachedHeader, true);

                    GUILayout.FlexibleSpace();

                    showMeta = GUILayout.Toggle(showMeta, s_ShowMeta, EditorStyles.miniButton);

                    if (GUILayout.Button(s_ResolveNow, EditorStyles.miniButton))
                        ResolveSelected(editor);
                }

                SessionState.SetBool(foldoutKey, expanded);

                if (!expanded)
                    return;

                EditorGUILayout.Space(4);

                DrawStatusLine(editor);

                var warnings = GetCachedFieldWarnings(type);
                if (warnings.Count > 0)
                {
                    string msg = string.Join("\n\u2022 ", warnings);
                    EditorGUILayout.HelpBox("CachedField configuration warnings:\n\u2022 " + msg, MessageType.Warning);

                    Debug.LogWarning(
                        $"{type.Name} has {warnings.Count} CachedField configuration warnings. See Inspector for details.");

                    EditorGUILayout.Space(4);
                }

                EditorGUILayout.Space(4);

                using (new EditorGUI.DisabledScope(true))
                {
                    DrawCachedFieldsReadOnly(editor, cached, showMeta);
                }
            }
        }

        private static void DrawStatusLine(UnityEditor.Editor editor)
        {
            bool anyErrors = false;
            bool anyUnresolved = false;

            for (int i = 0; i < editor.targets.Length; i++)
            {
                var c = editor.targets[i] as CacherMonoBehaviour;
                if (c == null)
                    continue;

                anyErrors |= c.LastResolveHadErrors;
                anyUnresolved |= !c.IsResolved;
            }

            string status =
                anyErrors ? "Status: Missing required references (see Console)." :
                anyUnresolved ? "Status: Not resolved yet." :
                "Status: OK.";

            var style = anyErrors ? EditorStyles.boldLabel : EditorStyles.miniLabel;
            EditorGUILayout.LabelField(status, style);
        }

        private static void ResolveSelected(UnityEditor.Editor editor)
        {
            Undo.RecordObjects(editor.targets, "Resolve Cached References");

            for (int i = 0; i < editor.targets.Length; i++)
            {
                if (editor.targets[i] is not CacherMonoBehaviour c || c == null)
                    continue;

                c.ResolveReferences();
                EditorUtility.SetDirty(c);
            }

            editor.serializedObject.Update();
            editor.Repaint();
        }

        private static void DrawCachedFieldsReadOnly(
            UnityEditor.Editor editor,
            List<CachedFieldInfo> cached,
            bool showMeta)
        {
            editor.serializedObject.Update();

            for (int i = 0; i < cached.Count; i++)
            {
                CachedFieldInfo info = cached[i];

                SerializedProperty prop = editor.serializedObject.FindProperty(info.Field.Name);

                if (prop != null)
                {
                    using (new EditorGUILayout.VerticalScope())
                    {
                        EditorGUILayout.PropertyField(prop, includeChildren: true);

                        if (showMeta)
                            DrawMeta(info.Attr);
                    }

                    EditorGUILayout.Space(2);
                    continue;
                }

                object value = info.Field.GetValue(editor.target);
                DrawFallbackValue(info.Field, value);

                if (showMeta)
                    DrawMeta(info.Attr);

                EditorGUILayout.Space(2);
            }

            editor.serializedObject.ApplyModifiedProperties();
        }

        private static void DrawMeta(CachedFieldAttribute attr)
        {
            EditorGUILayout.LabelField(
                $"Search: {attr.Search} | IncludeInactive: {attr.IncludeInactive} | Required: {attr.Required} | AddIfMissing: {attr.AddComponentIfMissing}" +
                (string.IsNullOrEmpty(attr.AddComponentIfMissingBoundToBoolField)
                    ? ""
                    : $" | AddIfMissingBool: {attr.AddComponentIfMissingBoundToBoolField}"),
                EditorStyles.miniLabel);
        }

        private static void DrawFallbackValue(FieldInfo field, object value)
        {
            string label = ObjectNames.NicifyVariableName(field.Name);

            if (value is UnityEngine.Object uo)
            {
                EditorGUILayout.ObjectField(label, uo, field.FieldType, allowSceneObjects: true);
                return;
            }

            if (value is IList list)
            {
                EditorGUILayout.LabelField(label, $"{field.FieldType.Name} (Count: {list.Count})",
                    EditorStyles.miniLabel);
                return;
            }

            EditorGUILayout.LabelField(label, value != null ? value.ToString() : "null");
        }

        internal static List<CachedFieldInfo> GetCachedFields(Type type)
        {
            if (s_CachedFieldsByType.TryGetValue(type, out var cached))
                return cached;

            var list = new List<CachedFieldInfo>(16);

            const BindingFlags flags =
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic |
                BindingFlags.DeclaredOnly;

            Type cur = type;

            while (cur != null && cur != typeof(MonoBehaviour))
            {
                FieldInfo[] fields = cur.GetFields(flags);

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldInfo f = fields[i];

                    var attr = f.GetCustomAttribute<CachedFieldAttribute>();
                    if (attr == null)
                        continue;

                    list.Add(new CachedFieldInfo
                    {
                        Field = f,
                        Attr = attr
                    });
                }

                cur = cur.BaseType;
            }

            list.Sort((a, b) => string.CompareOrdinal(a.Field.Name, b.Field.Name));

            s_CachedFieldsByType[type] = list;
            return list;
        }

        private static List<string> GetCachedFieldWarnings(Type inspectedRuntimeType)
        {
            if (s_WarningCacheByType.TryGetValue(inspectedRuntimeType, out var cached))
                return cached;

            var warnings = new List<string>();

            var cachedFields = GetCachedFields(inspectedRuntimeType);

            for (int i = 0; i < cachedFields.Count; i++)
            {
                FieldInfo f = cachedFields[i].Field;

                bool isSerializedByUnity =
                    f.IsPublic ||
                    f.GetCustomAttribute<SerializeField>() != null;

                if (!isSerializedByUnity)
                {
                    warnings.Add(
                        $"'{f.DeclaringType.Name}.{f.Name}' is [CachedField] but not serialized. Add [SerializeField] (or make it public).");
                }

                Type declaring = f.DeclaringType;
                if (f.IsPrivate && declaring != null && !declaring.IsSealed)
                {
                    warnings.Add(
                        $"'{declaring.Name}.{f.Name}' is private and [CachedField] on a non-sealed class. Derived types will not resolve this field with the current resolver. Make it protected (recommended) or seal '{declaring.Name}', or update resolver to walk base types.");
                }
            }

            s_WarningCacheByType[inspectedRuntimeType] = warnings;
            return warnings;
        }
    }

#if ODIN_INSPECTOR
    public sealed class CachedFieldAttributeProcessor : OdinAttributeProcessor<CacherMonoBehaviour>
    {
        public override void ProcessChildMemberAttributes(
            InspectorProperty parentProperty,
            MemberInfo member,
            List<Attribute> attributes)
        {
            if (member.GetCustomAttribute<CachedFieldAttribute>() != null)
                attributes.Add(new HideInInspector());
        }
    }

    [CustomEditor(typeof(CacherMonoBehaviour), true)]
    [CanEditMultipleObjects]
    public sealed class CacherEditor : OdinEditor
    {
        private bool m_ShowMeta;

        protected override void DrawTree()
        {
            base.DrawTree();

            EditorGUILayout.Space(8);
            CacherEditorHelper.DrawCachedSection(this, ref m_ShowMeta);
            GUILayout.Space(10);
        }
    }
#else
    [CustomEditor(typeof(CacherMonoBehaviour), true)]
    [CanEditMultipleObjects]
    public sealed class CacherEditor : UnityEditor.Editor
    {
        private bool m_ShowMeta;
        private HashSet<string> m_CachedFieldNames;

        private void OnEnable()
        {
            var t0 = target as CacherMonoBehaviour;
            if (t0 == null)
                return;

            var cached = CacherEditorHelper.GetCachedFields(t0.GetType());
            m_CachedFieldNames = new HashSet<string>(cached.Count);
            for (int i = 0; i < cached.Count; i++)
                m_CachedFieldNames.Add(cached[i].Field.Name);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            SerializedProperty prop = serializedObject.GetIterator();
            bool enterChildren = true;

            while (prop.NextVisible(enterChildren))
            {
                enterChildren = false;

                if (prop.name == "m_Script")
                {
                    using (new EditorGUI.DisabledScope(true))
                        EditorGUILayout.PropertyField(prop);
                    continue;
                }

                if (m_CachedFieldNames != null && m_CachedFieldNames.Contains(prop.name))
                    continue;

                EditorGUILayout.PropertyField(prop, includeChildren: true);
            }

            serializedObject.ApplyModifiedProperties();

            EditorGUILayout.Space(8);
            CacherEditorHelper.DrawCachedSection(this, ref m_ShowMeta);
            GUILayout.Space(10);
        }
    }
#endif

}
#endif