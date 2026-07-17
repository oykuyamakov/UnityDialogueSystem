#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Cadi.Scripts.CustomAttributes.Editor
{
    internal static class ButtonDrawerUtility
    {
        private static readonly HashSet<Type> s_SupportedTypes = new()
        {
            typeof(int),
            typeof(float),
            typeof(bool),
            typeof(string),
            typeof(Vector2),
            typeof(Vector3),
            typeof(Vector4),
            typeof(Color),
        };
        
        private sealed class ButtonBinding
        {
            public MethodInfo Method;
            public ButtonAttribute Attr;
            public ParameterInfo[] Parameters;
            public string Label;
        }

        private static readonly Dictionary<Type, List<ButtonBinding>> s_ButtonCache = new();

        private static List<ButtonBinding> GetButtonsForType(Type type)
        {
            if (s_ButtonCache.TryGetValue(type, out var cached))
                return cached;

            var list = new List<ButtonBinding>();

            // Walk the hierarchy with DeclaredOnly so private [Button] methods on
            // base classes are found too (GetMethods(NonPublic) misses them) —
            // same reasoning as GetBindingsForType in CacherMonoBehaviour.
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public |
                                       BindingFlags.NonPublic | BindingFlags.DeclaredOnly;

            var seen = new HashSet<string>();
            Type cur = type;
            while (cur != null && cur != typeof(MonoBehaviour) && cur != typeof(ScriptableObject) && cur != typeof(object))
            {
                foreach (var m in cur.GetMethods(flags))
                {
                    if (m.IsGenericMethodDefinition)
                        continue; // can't invoke without type args

                    var attr = m.GetCustomAttribute<ButtonAttribute>();
                    if (attr == null)
                        continue;

                    if (!seen.Add(m.Name + m.GetParameters().Length)) // overridden methods: keep most-derived
                        continue;

                    list.Add(new ButtonBinding
                    {
                        Method = m,
                        Attr = attr,
                        Parameters = m.GetParameters(),
                        Label = string.IsNullOrEmpty(attr.Label)
                            ? ObjectNames.NicifyVariableName(m.Name)
                            : attr.Label
                    });
                }
                cur = cur.BaseType;
            }

            s_ButtonCache[type] = list; // empty list cached too — that's the fast early-out
            return list;
        }

        public static void DrawButtons(UnityEditor.Editor editor)
        {
            var targetObj = editor.target;
            if (targetObj == null)
                return;

            var buttons = GetButtonsForType(targetObj.GetType());
            if (buttons.Count == 0)
                return; // ← the common case, now one dictionary hit

            foreach (var b in buttons)
            {
                if (b.Parameters.Length == 0)
                {
                    if (GUILayout.Button(b.Label))
                        InvokeOnAllTargets(editor, b.Method, Array.Empty<object>());
                    continue;
                }

                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    EditorGUILayout.LabelField(b.Label, EditorStyles.boldLabel);

                    if (!TryDrawParameterFields(targetObj.GetType(), b.Method, b.Parameters, out var args))
                    {
                        EditorGUILayout.HelpBox(
                            $"[button] unsupported parameter type(s) on method: {b.Method.Name}",
                            MessageType.Warning);
                        continue;
                    }

                    using (new EditorGUI.DisabledScope(editor.targets == null || editor.targets.Length == 0))
                    {
                        if (GUILayout.Button("Run"))
                            InvokeOnAllTargets(editor, b.Method, args);
                    }
                }
            }
        }
        
        [UnityEditor.Callbacks.DidReloadScripts]
        private static void ClearCache() => s_ButtonCache.Clear();

        private static void InvokeOnAllTargets(UnityEditor.Editor editor, MethodInfo method, object[] args)
        {
            var targets = editor.targets;
            if (targets == null || targets.Length == 0)
            {
                return;
            }

            foreach (var t in targets)
            {
                if (t == null)
                    continue;

                try
                {
                    Undo.RecordObject(t, $"Invoke {method.Name}");
                    method.Invoke(t, args);
                    EditorUtility.SetDirty(t);
                }
                catch (Exception e)
                {
                    Debug.LogError($"[button] invoke failed on {t.name}.{method.Name}: {e}");
                }
            }
        }

        private static bool TryDrawParameterFields(Type targetType, MethodInfo method,
            ParameterInfo[] parameters, out object[] args)
        {
            args = new object[parameters.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                var p = parameters[i];
                var pType = p.ParameterType;

                var paramLabel = ObjectNames.NicifyVariableName(p.Name);
                var storageKey = MakeKey(targetType, method, p);

                if (pType.IsEnum)
                {
                    var enumValue = (Enum)LoadValue(storageKey, pType, GetDefaultEnum(pType, p));
                    enumValue = EditorGUILayout.EnumPopup(paramLabel, enumValue);
                    SaveValue(storageKey, enumValue);
                    args[i] = enumValue;
                    continue;
                }

                if (typeof(Object).IsAssignableFrom(pType))
                {
                    // Object refs are not persisted (by design)
                    var obj = (Object)LoadValue(storageKey, pType, null);
                    obj = EditorGUILayout.ObjectField(paramLabel, obj, pType, true);
                    SaveValue(storageKey, obj);
                    args[i] = obj;
                    continue;
                }

                if (!s_SupportedTypes.Contains(pType))
                {
                    return false;
                }

                if (pType == typeof(int))
                {
                    var v = (int)LoadValue(storageKey, pType, GetDefaultInt(p));
                    v = EditorGUILayout.IntField(paramLabel, v);
                    SaveValue(storageKey, v);
                    args[i] = v;
                    continue;
                }

                if (pType == typeof(float))
                {
                    var v = (float)LoadValue(storageKey, pType, GetDefaultFloat(p));
                    v = EditorGUILayout.FloatField(paramLabel, v);
                    SaveValue(storageKey, v);
                    args[i] = v;
                    continue;
                }

                if (pType == typeof(bool))
                {
                    var v = (bool)LoadValue(storageKey, pType, GetDefaultBool(p));
                    v = EditorGUILayout.Toggle(paramLabel, v);
                    SaveValue(storageKey, v);
                    args[i] = v;
                    continue;
                }

                if (pType == typeof(string))
                {
                    var v = (string)LoadValue(storageKey, pType, GetDefaultString(p));
                    v = EditorGUILayout.TextField(paramLabel, v);
                    SaveValue(storageKey, v);
                    args[i] = v;
                    continue;
                }

                if (pType == typeof(Vector2))
                {
                    var v = (Vector2)LoadValue(storageKey, pType, Vector2.zero);
                    v = EditorGUILayout.Vector2Field(paramLabel, v);
                    SaveValue(storageKey, v);
                    args[i] = v;
                    continue;
                }

                if (pType == typeof(Vector3))
                {
                    var v = (Vector3)LoadValue(storageKey, pType, Vector3.zero);
                    v = EditorGUILayout.Vector3Field(paramLabel, v);
                    SaveValue(storageKey, v);
                    args[i] = v;
                    continue;
                }

                if (pType == typeof(Vector4))
                {
                    var v = (Vector4)LoadValue(storageKey, pType, Vector4.zero);
                    v = EditorGUILayout.Vector4Field(paramLabel, v);
                    SaveValue(storageKey, v);
                    args[i] = v;
                    continue;
                }

                // Color
                {
                    var v = (Color)LoadValue(storageKey, pType, Color.white);
                    v = EditorGUILayout.ColorField(paramLabel, v);
                    SaveValue(storageKey, v);
                    args[i] = v;
                }
            }

            return true;
        }

        // ---------- Defaults ----------

        private static int GetDefaultInt(ParameterInfo p)
        {
            if (TryGetDefaultFromMetadata(p, out var v) && v is int iv) return iv;
            if (TryGetDefaultFromMetadata(p, out v) && v is short sv) return sv;
            if (TryGetDefaultFromMetadata(p, out v) && v is long lv) return (int)lv;
            return 0;
        }

        private static float GetDefaultFloat(ParameterInfo p)
        {
            if (TryGetDefaultFromMetadata(p, out var v) && v is float fv) return fv;
            if (TryGetDefaultFromMetadata(p, out v) && v is double dv) return (float)dv;
            if (TryGetDefaultFromMetadata(p, out v) && v is int iv) return iv;
            return 0f;
        }

        private static bool GetDefaultBool(ParameterInfo p)
        {
            if (TryGetDefaultFromMetadata(p, out var v) && v is bool bv) return bv;
            return false;
        }

        private static string GetDefaultString(ParameterInfo p)
        {
            if (TryGetDefaultFromMetadata(p, out var v) && v is string sv) return sv;
            return string.Empty;
        }

        private static Enum GetDefaultEnum(Type enumType, ParameterInfo p)
        {
            if (TryGetDefaultFromMetadata(p, out var v) && v != null)
            {
                try
                {
                    if (v.GetType().IsEnum) return (Enum)v;
                    var underlying = Convert.ToInt32(v, CultureInfo.InvariantCulture);
                    return (Enum)Enum.ToObject(enumType, underlying);
                }
                catch
                {
                    // ignore
                }
            }

            var values = Enum.GetValues(enumType);
            return (Enum)values.GetValue(0);
        }

        private static bool TryGetDefaultFromMetadata(ParameterInfo p, out object value)
        {
            value = null;

            try
            {
                if (p.HasDefaultValue)
                {
                    value = p.DefaultValue;
                    return true;
                }
            }
            catch
            {
                // ignore
            }

            return false;
        }

        // ---------- Persistence ----------

        private static string MakeKey(Type targetType, MethodInfo method, ParameterInfo p)
        {
            return $"ButtonArgs:{targetType.FullName}:{method.Name}:{p.Name}:{p.ParameterType.FullName}";
        }

        private static object LoadValue(string key, Type type, object fallback)
        {
            if (type == typeof(int))
            {
                return EditorPrefs.GetInt(key, fallback is int fi ? fi : 0);
            }

            if (type == typeof(float))
            {
                var s = EditorPrefs.GetString(key, (fallback is float ff ? ff : 0f).ToString(CultureInfo.InvariantCulture));
                if (float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out var v))
                {
                    return v;
                }

                return fallback is float ffb ? ffb : 0f;
            }

            if (type == typeof(bool))
            {
                return EditorPrefs.GetBool(key, fallback is bool fb && fb);
            }

            if (type == typeof(string))
            {
                return EditorPrefs.GetString(key, fallback as string ?? string.Empty);
            }

            if (type == typeof(Vector2))
            {
                return ParseVector(key, 2, fallback is Vector2 fv ? fv : Vector2.zero);
            }

            if (type == typeof(Vector3))
            {
                return ParseVector(key, 3, fallback is Vector3 fv ? fv : Vector3.zero);
            }

            if (type == typeof(Vector4))
            {
                return ParseVector(key, 4, fallback is Vector4 fv ? fv : Vector4.zero);
            }

            if (type == typeof(Color))
            {
                var c = fallback is Color fc ? fc : Color.white;
                var raw = EditorPrefs.GetString(key, $"{c.r},{c.g},{c.b},{c.a}");
                var parts = raw.Split(',');
                if (parts.Length == 4 &&
                    float.TryParse(parts[0], NumberStyles.Float, CultureInfo.InvariantCulture, out var r) &&
                    float.TryParse(parts[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var g) &&
                    float.TryParse(parts[2], NumberStyles.Float, CultureInfo.InvariantCulture, out var b) &&
                    float.TryParse(parts[3], NumberStyles.Float, CultureInfo.InvariantCulture, out var a))
                {
                    return new Color(r, g, b, a);
                }

                return c;
            }

            if (type.IsEnum)
            {
                var raw = EditorPrefs.GetInt(key,
                    fallback != null ? Convert.ToInt32(fallback, CultureInfo.InvariantCulture) : 0);
                return Enum.ToObject(type, raw);
            }

            // UnityEngine.Object refs not persisted by design.
            return fallback;
        }

        private static void SaveValue(string key, object value)
        {
            if (value is int iv)
            {
                EditorPrefs.SetInt(key, iv);
                return;
            }

            if (value is float fv)
            {
                EditorPrefs.SetString(key, fv.ToString(CultureInfo.InvariantCulture));
                return;
            }

            if (value is bool bv)
            {
                EditorPrefs.SetBool(key, bv);
                return;
            }

            if (value is string sv)
            {
                EditorPrefs.SetString(key, sv ?? string.Empty);
                return;
            }

            if (value is Vector2 v2)
            {
                EditorPrefs.SetString(key, $"{v2.x},{v2.y}");
                return;
            }

            if (value is Vector3 v3)
            {
                EditorPrefs.SetString(key, $"{v3.x},{v3.y},{v3.z}");
                return;
            }

            if (value is Vector4 v4)
            {
                EditorPrefs.SetString(key, $"{v4.x},{v4.y},{v4.z},{v4.w}");
                return;
            }

            if (value is Color c)
            {
                EditorPrefs.SetString(key, $"{c.r},{c.g},{c.b},{c.a}");
                return;
            }

            if (value is Enum ev)
            {
                EditorPrefs.SetInt(key, Convert.ToInt32(ev, CultureInfo.InvariantCulture));
                return;
            }

            // UnityEngine.Object refs not persisted by design.
        }

        private static object ParseVector(string key, int dims, object fallback)
        {
            var raw = EditorPrefs.GetString(key, null);
            if (string.IsNullOrEmpty(raw))
            {
                return fallback;
            }

            var parts = raw.Split(',');
            if (parts.Length != dims)
            {
                return fallback;
            }

            var vals = new float[dims];
            for (int i = 0; i < dims; i++)
            {
                if (!float.TryParse(parts[i], NumberStyles.Float, CultureInfo.InvariantCulture, out vals[i]))
                {
                    return fallback;
                }
            }

            if (dims == 2) return new Vector2(vals[0], vals[1]);
            if (dims == 3) return new Vector3(vals[0], vals[1], vals[2]);
            return new Vector4(vals[0], vals[1], vals[2], vals[3]);
        }
    }
}
#endif