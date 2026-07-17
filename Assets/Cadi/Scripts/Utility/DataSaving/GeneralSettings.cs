using System.IO;
using UnityEditor;
using UnityEngine;

namespace Cadi.Scripts.Utility.DataSaving
{
    [CreateAssetMenu(fileName = "GeneralSettings", menuName = "GeneralSettings", order = 0)]
    public class GeneralSettings : ScriptableObject
    {
        private const string c_ResourcesAssetPath = "GeneralSettings"; // Resources/GeneralSettings.asset
        private static GeneralSettings s_GeneralSettings;

        public static GeneralSettings Get()
        {
            if (s_GeneralSettings != null)
            {
                return s_GeneralSettings;
            }

            s_GeneralSettings = Resources.Load<GeneralSettings>(c_ResourcesAssetPath);

#if UNITY_EDITOR
            if (s_GeneralSettings != null)
                return s_GeneralSettings;

            Debug.Log($"Content not found. Creating a new one at Resources/ {c_ResourcesAssetPath}");
            s_GeneralSettings = CreateInstance<GeneralSettings>();
            EnsureAssetExists(s_GeneralSettings);
#else
    if (s_GeneralSettings == null)
        Debug.LogError($"{c_ResourcesAssetPath} not found in Resources — was it ever created in editor?");
#endif

            return s_GeneralSettings;
        }


#if UNITY_EDITOR

        private static void EnsureAssetExists(GeneralSettings generalSettings)
        {
            const string assetPath = "Assets/Resources/GeneralSettings.asset";
            EnsureFolderExists("Assets/Resources");

            AssetDatabase.CreateAsset(generalSettings, assetPath);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

        private static void EnsureFolderExists(string folderPath)
        {
            if (AssetDatabase.IsValidFolder(folderPath))
            {
                return;
            }

            string parent = Path.GetDirectoryName(folderPath)?.Replace("\\", "/");
            string name = Path.GetFileName(folderPath);

            if (!AssetDatabase.IsValidFolder(parent))
            {
                EnsureFolderExists(parent);
            }

            AssetDatabase.CreateFolder(parent, name);
        }

#endif
    }
}