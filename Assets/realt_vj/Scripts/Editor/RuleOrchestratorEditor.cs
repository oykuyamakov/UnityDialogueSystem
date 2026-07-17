using System;
using System.Collections.Generic;
using System.Linq;
using RealtVJ.Data;
using RealtVJ.Runtime;
using UnityEditor;
using UnityEngine;

namespace RealtVJ.Editor
{
    [CustomEditor(typeof(RuleOrchestrator))]
    public class RuleOrchestratorEditor : UnityEditor.Editor
    {
        private SerializedProperty m_GraphProp;
        private SerializedProperty m_BindingsProp;

        private void OnEnable()
        {
            m_GraphProp = serializedObject.FindProperty("m_Graph");
            m_BindingsProp = serializedObject.FindProperty("m_Bindings");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(m_GraphProp, new GUIContent("Rule Graph"));

            var orchestrator = (RuleOrchestrator)target;
            var graph = orchestrator.Graph;

            if (graph == null)
            {
                EditorGUILayout.HelpBox("Assign a Rule Graph to see result bindings.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Result Bindings", EditorStyles.boldLabel);

            // Collect all results from the graph
            var entries = orchestrator.GetAllResultEntries().ToList();

            if (entries.Count == 0)
            {
                EditorGUILayout.HelpBox("No results found in the graph.", MessageType.Info);
                serializedObject.ApplyModifiedProperties();
                return;
            }

            // Build a lookup of existing bindings by GUID
            var bindingsByGuid = new Dictionary<string, int>();
            for (int i = 0; i < m_BindingsProp.arraySize; i++)
            {
                var guidProp = m_BindingsProp.GetArrayElementAtIndex(i).FindPropertyRelative("ResultGuid");
                if (guidProp != null && !string.IsNullOrEmpty(guidProp.stringValue))
                    bindingsByGuid[guidProp.stringValue] = i;
            }

            bool changed = false;

            // Ensure every result in the graph has a binding slot
            foreach (var (guid, typeName, displayName) in entries)
            {
                if (!bindingsByGuid.ContainsKey(guid))
                {
                    m_BindingsProp.InsertArrayElementAtIndex(m_BindingsProp.arraySize);
                    var newElement = m_BindingsProp.GetArrayElementAtIndex(m_BindingsProp.arraySize - 1);
                    newElement.FindPropertyRelative("ResultGuid").stringValue = guid;
                    newElement.FindPropertyRelative("Executor").objectReferenceValue = null;
                    bindingsByGuid[guid] = m_BindingsProp.arraySize - 1;
                    changed = true;
                }
            }

            // Remove bindings for results that no longer exist in the graph
            var validGuids = new HashSet<string>(entries.Select(e => e.guid));
            for (int i = m_BindingsProp.arraySize - 1; i >= 0; i--)
            {
                var guidProp = m_BindingsProp.GetArrayElementAtIndex(i).FindPropertyRelative("ResultGuid");
                if (guidProp == null || !validGuids.Contains(guidProp.stringValue))
                {
                    m_BindingsProp.DeleteArrayElementAtIndex(i);
                    changed = true;
                }
            }

            if (changed)
            {
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                // Rebuild lookup after changes
                bindingsByGuid.Clear();
                for (int i = 0; i < m_BindingsProp.arraySize; i++)
                {
                    var guidProp = m_BindingsProp.GetArrayElementAtIndex(i).FindPropertyRelative("ResultGuid");
                    if (guidProp != null && !string.IsNullOrEmpty(guidProp.stringValue))
                        bindingsByGuid[guidProp.stringValue] = i;
                }
            }

            // Draw each binding
            foreach (var (guid, typeName, displayName) in entries)
            {
                if (!bindingsByGuid.TryGetValue(guid, out int bindingIndex)) continue;

                var element = m_BindingsProp.GetArrayElementAtIndex(bindingIndex);
                var executorProp = element.FindPropertyRelative("Executor");

                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField($"[{displayName}]", GUILayout.Width(140));

                // Get the expected executor type for validation
                var resultType = GetResultTypeFromName(typeName);
                var currentExecutor = executorProp.objectReferenceValue as ResultExecutor;

                // Validate type match
                if (currentExecutor != null && resultType != null && currentExecutor.HandledResultType != resultType)
                {
                    EditorGUILayout.HelpBox("Type mismatch!", MessageType.Error);
                    executorProp.objectReferenceValue = null;
                }

                EditorGUILayout.PropertyField(executorProp, GUIContent.none);
                EditorGUILayout.EndHorizontal();

                // Show short GUID for debugging
                var shortGuid = guid.Length > 8 ? guid.Substring(0, 8) + "..." : guid;
                EditorGUI.indentLevel++;
                EditorGUILayout.LabelField($"GUID: {shortGuid}", EditorStyles.miniLabel);
                EditorGUI.indentLevel--;
            }

            if (GUILayout.Button("Refresh Bindings"))
            {
                // Force re-sync by clearing and rebuilding
                m_BindingsProp.ClearArray();
                serializedObject.ApplyModifiedProperties();
                serializedObject.Update();
                Repaint();
                return;
            }

            serializedObject.ApplyModifiedProperties();
        }

        private static Type GetResultTypeFromName(string typeName)
        {
            var baseType = typeof(Result);
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.Name == typeName && baseType.IsAssignableFrom(type))
                            return type;
                    }
                }
                catch { }
            }
            return null;
        }
    }
}
