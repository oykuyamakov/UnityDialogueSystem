#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cadi.Scripts.CacherSystem.Editor
{
    public static class CacherBehavioursResolveMenu
    {
        private const string c_MenuResolveBuildScenes = "Tools/Auto References/Resolve (Build Scenes)";
        private const string c_MenuResolveOpenScenes  = "Tools/Auto References/Resolve (Open Scenes)";

        [MenuItem(c_MenuResolveBuildScenes, priority = 100)]
        private static void ResolveBuildScenes()
        {
            if (!EditorUtility.DisplayDialog(
                    "Resolve Auto References",
                    "This will open each enabled Build Settings scene, resolve [CachedField] references, and optionally save the scenes.\n\nProceed?",
                    "Proceed",
                    "Cancel"))
            {
                return;
            }

            bool saveScenes = EditorUtility.DisplayDialog(
                "Save Scenes?",
                "After resolving, do you want to save any modified scenes?",
                "Save",
                "Don't Save");

            var previousSetup = EditorSceneManager.GetSceneManagerSetup();

            int totalComponents = 0;
            int errorComponents = 0;

            try
            {
                var scenes = EditorBuildSettings.scenes;
                var enabledScenes = new List<EditorBuildSettingsScene>(scenes.Length);

                for (int i = 0; i < scenes.Length; i++)
                {
                    if (scenes[i].enabled)
                        enabledScenes.Add(scenes[i]);
                }

                for (int i = 0; i < enabledScenes.Count; i++)
                {
                    string path = enabledScenes[i].path;

                    EditorUtility.DisplayProgressBar(
                        "Resolving Auto References",
                        $"Opening scene {i + 1}/{enabledScenes.Count}\n{path}",
                        enabledScenes.Count == 0 ? 1f : (float)i / enabledScenes.Count);

                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    ResolveScene(scene, ref totalComponents, ref errorComponents);

                    if (saveScenes && scene.isDirty)
                        EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }

            if (errorComponents > 0)
            {
                EditorUtility.DisplayDialog(
                    "Resolve Complete (Errors)",
                    $"Resolved {totalComponents} component(s).\nErrors: {errorComponents}\n\nCheck Console for details.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Resolve Complete",
                    $"Resolved {totalComponents} component(s).\nNo missing required references detected.",
                    "OK");
            }
        }

        [MenuItem(c_MenuResolveOpenScenes, priority = 101)]
        private static void ResolveOpenScenes()
        {
            if (!EditorUtility.DisplayDialog(
                    "Resolve Auto References",
                    "This will resolve [CachedField] references in all currently open scenes.\n\nProceed?",
                    "Proceed",
                    "Cancel"))
            {
                return;
            }

            bool saveScenes = EditorUtility.DisplayDialog(
                "Save Scenes?",
                "After resolving, do you want to save any modified open scenes?",
                "Save",
                "Don't Save");

            int totalComponents = 0;
            int errorComponents = 0;

            try
            {
                int sceneCount = SceneManager.sceneCount;

                for (int i = 0; i < sceneCount; i++)
                {
                    Scene scene = SceneManager.GetSceneAt(i);
                    if (!scene.isLoaded)
                        continue;

                    EditorUtility.DisplayProgressBar(
                        "Resolving Auto References",
                        $"Resolving open scene {i + 1}/{sceneCount}\n{scene.path}",
                        sceneCount == 0 ? 1f : (float)i / sceneCount);

                    ResolveScene(scene, ref totalComponents, ref errorComponents);

                    if (saveScenes && scene.isDirty)
                        EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }

            if (errorComponents > 0)
            {
                EditorUtility.DisplayDialog(
                    "Resolve Complete (Errors)",
                    $"Resolved {totalComponents} component(s).\nErrors: {errorComponents}\n\nCheck Console for details.",
                    "OK");
            }
            else
            {
                EditorUtility.DisplayDialog(
                    "Resolve Complete",
                    $"Resolved {totalComponents} component(s).\nNo missing required references detected.",
                    "OK");
            }
        }

        private static void ResolveScene(Scene scene, ref int totalComponents, ref int errorComponents)
        {
            GameObject[] roots = scene.GetRootGameObjects();

            for (int r = 0; r < roots.Length; r++)
            {
                var resolvers = roots[r].GetComponentsInChildren<CacherMonoBehaviour>(true);

                for (int j = 0; j < resolvers.Length; j++)
                {
                    var comp = resolvers[j];
                    if (comp == null)
                        continue;

                    comp.ResolveReferences();
                    totalComponents++;

                    if (comp.LastResolveHadErrors)
                        errorComponents++;
                }
            }
        }
    }
}
#endif



