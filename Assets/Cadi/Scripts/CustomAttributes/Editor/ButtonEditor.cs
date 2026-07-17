#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

namespace Cadi.Scripts.CustomAttributes.Editor
{
    /// <summary>
    /// Marker interface for editors that already call ButtonDrawerUtility.DrawButtons
    /// in their OnInspectorGUI. The CadiButtonHeaderHook skips these to prevent double-drawing.
    /// </summary>
    public interface ICadiButtonEditor { }

    /// <summary>
    /// Fallback hook for when Odin (or any other package) overrides the MonoBehaviour/ScriptableObject
    /// custom editor and our isFallback editors never run. Fires from DrawHeader(), which all editors
    /// must call to render the standard component header bar.
    /// </summary>
#if !ODIN_INSPECTOR
    [InitializeOnLoad]
    internal static class CadiButtonHeaderHook
    {
        static CadiButtonHeaderHook()
        {
            UnityEditor.Editor.finishedDefaultHeaderGUI += OnFinishedHeaderGUI;
        }

        private static void OnFinishedHeaderGUI(UnityEditor.Editor editor)
        {
            // Skip editors that already handle button drawing in their OnInspectorGUI
            if (editor is ICadiButtonEditor) return;

            if (editor.target == null) return;
            if (editor.target is not MonoBehaviour && editor.target is not ScriptableObject) return;

            try
            {
                ButtonDrawerUtility.DrawButtons(editor);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }

    [CustomEditor(typeof(MonoBehaviour), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class ButtonMonoBehaviourEditor : UnityEditor.Editor, ICadiButtonEditor
    {
        public override void OnInspectorGUI()
        {
            if (target == null) return;
            DrawDefaultInspector();
            GUILayout.Space(10);
            ButtonDrawerUtility.DrawButtons(this);
        }
    }

    [CustomEditor(typeof(ScriptableObject), true, isFallback = true)]
    [CanEditMultipleObjects]
    public class ButtonScriptableObjectEditor : UnityEditor.Editor, ICadiButtonEditor
    {
        public override void OnInspectorGUI()
        {
            if (target == null) return;
            DrawDefaultInspector();
            GUILayout.Space(10);
            ButtonDrawerUtility.DrawButtons(this);
        }
    }
#endif // !ODIN_INSPECTOR
}
#endif // UNITY_EDITOR