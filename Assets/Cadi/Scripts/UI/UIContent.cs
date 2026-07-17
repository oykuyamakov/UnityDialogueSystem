using System.Collections.Generic;
using System.IO;
using System.Linq;
using Cadi.Scripts.UI.FX;
using UnityEditor;
using UnityEngine;

namespace Cadi.Scripts.UI
{
    [CreateAssetMenu(fileName = "UIContent", menuName = "Content/UI", order = 0)]
    public class UIContent : ScriptableObject
    {
        private const string c_AssetResourcePath = "Content/UIContent";

        private static UIContent s_Content;

        public static UIContent Get()
        {
            if (s_Content != null)
            {
                return s_Content;
            }

            s_Content = Resources.Load<UIContent>(c_AssetResourcePath);

#if UNITY_EDITOR
            if (s_Content != null)
                return s_Content;

            Debug.Log("UI Content not found. Creating a new one at Assets/Resources/UIContent.asset");
            s_Content = CreateInstance<UIContent>();
            EnsureAssetExists(s_Content);
#endif

            return s_Content;
        }

        [Header("UI Prefabs")]
        [SerializeField]
        private List<FXGraphix> m_UiFxContents = new();

        public IReadOnlyList<FXGraphix> UiFxContents => m_UiFxContents;

        private Dictionary<UIFxType, FXGraphix> m_UiFxLookup;

        public FXGraphix Get(UIFxType type)
        {
            if (m_UiFxLookup == null)
            {
                SetUpLookUp();
            }

            if (m_UiFxLookup != null && m_UiFxLookup.TryGetValue(type, out var content))
            {
                return content;
            }

            Debug.LogWarning($"UI Fx of type {type} not found in Content.");
            return null;
        }

        private void SetUpLookUp()
        {
            if (m_UiFxContents == null || m_UiFxContents.Count == 0)
            {
                m_UiFxLookup = new Dictionary<UIFxType, FXGraphix>();
                return;
            }

            m_UiFxLookup = m_UiFxContents.ToDictionary(c => c.FxType, c => c);
        }


#if UNITY_EDITOR
        private static void EnsureAssetExists(UIContent content)
        {
            const string folderPath = "Assets/Resources/Content";

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                Debug.Log(folderPath);
                AssetDatabase.CreateFolder("Assets/Resources", "Content");
            }

            AssetDatabase.CreateAsset(content, "Assets/Resources/" + c_AssetResourcePath + ".asset");
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