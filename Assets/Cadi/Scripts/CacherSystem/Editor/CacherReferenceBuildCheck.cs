#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Cadi.Scripts.CacherSystem
{
    public sealed class CacherReferenceBuildCheck : IPreprocessBuildWithReport
    {
        public int callbackOrder => 0;

        public void OnPreprocessBuild(BuildReport report)
        {
            var previousSetup = EditorSceneManager.GetSceneManagerSetup();

            try
            {
                var scenes = EditorBuildSettings.scenes;

                for (int i = 0; i < scenes.Length; i++)
                {
                    if (!scenes[i].enabled)
                    {
                        continue;
                    }

                    string path = scenes[i].path;
                    Scene scene = EditorSceneManager.OpenScene(path, OpenSceneMode.Single);

                    GameObject[] roots = scene.GetRootGameObjects();

                    for (int r = 0; r < roots.Length; r++)
                    {
                        var resolvers = roots[r].GetComponentsInChildren<CacherMonoBehaviour>(true);

                        for (int b = 0; b < resolvers.Length; b++)
                        {
                            var comp = resolvers[b];

                            comp.ResolveReferences();

                            if (comp.LastResolveHadErrors)
                            {
                                throw new BuildFailedException(
                                    $"AutoReference build check failed. Missing required references on: {comp.name} ({comp.GetType().Name}) in scene: {path}");
                            }
                            
                            //only set dirty if there is not resolved ones
                            if (comp.LastResolveChangedAnything)
                                EditorUtility.SetDirty(comp);
                        }
                    }
                    
                    // persist the resolved state so the build ships it.
                    if (scene.isDirty)
                        EditorSceneManager.SaveScene(scene);
                }
            }
            finally
            {
                EditorSceneManager.RestoreSceneManagerSetup(previousSetup);
            }
            
            
            string[] guids = AssetDatabase.FindAssets("t:Prefab");
            foreach (string guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var root = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (root == null || root.GetComponentInChildren<CacherMonoBehaviour>(true) == null)
                    continue;

                using var scope = new PrefabUtility.EditPrefabContentsScope(path);
                foreach (var c in scope.prefabContentsRoot.GetComponentsInChildren<CacherMonoBehaviour>(true))
                {
                    c.ResolveReferences();
                    if (c.LastResolveHadErrors)
                    {
                        throw new BuildFailedException(
                            $"AutoReference build check failed. Missing required references on prefabs: {c.name} ({c.GetType().Name}) in scene: {path}");
                    }
                }
                // scope disposal saves the prefab if modified
            }
        }
    }
}
#endif