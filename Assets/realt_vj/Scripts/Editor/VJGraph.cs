using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealtVJ.Editor
{
    public class VJGraph : EditorWindow
    {
        private VJGraphView m_GraphView;
        private TextField m_FileNameTextField;
        private string m_FileName = "NewRuleGraph";

        [MenuItem("Graph/VJ Rule Graph")]
        public static void OpenVJGraphWindow()
        {
            var window = GetWindow<VJGraph>();
            window.titleContent = new GUIContent("VJ Rule Graph");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
        }

        private void ConstructGraphView()
        {
            m_GraphView = new VJGraphView(this)
            {
                name = "VJ Rule Graph"
            };

            m_GraphView.StretchToParentSize();
            rootVisualElement.Add(m_GraphView);
        }

        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();

            var fileLabel = new Label("File Name:");
            fileLabel.style.marginLeft = 10;
            fileLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            fileLabel.style.unityTextAlign = TextAnchor.MiddleLeft;
            toolbar.Add(fileLabel);

            m_FileNameTextField = new TextField { value = m_FileName };
            m_FileNameTextField.RegisterValueChangedCallback(evt => m_FileName = evt.newValue);
            m_FileNameTextField.style.width = 200;
            toolbar.Add(m_FileNameTextField);

            var saveButton = new Button(() => RequestDataOperation(true));
            saveButton.text = "Save";
            toolbar.Add(saveButton);

            var loadButton = new Button(() => RequestDataOperation(false));
            loadButton.text = "Load";
            toolbar.Add(loadButton);

            var miniMapToggle = new Toggle("Minimap");
            miniMapToggle.value = true;
            miniMapToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    m_GraphView.InitializeMiniMap();
                else
                    m_GraphView.RemoveMiniMap();
            });
            miniMapToggle.style.marginLeft = 20;
            miniMapToggle.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolbar.Add(miniMapToggle);

            var groupButton = new Button(() => m_GraphView.GroupSelectedNodes());
            groupButton.text = "Group Selected";
            groupButton.style.marginLeft = 10;
            toolbar.Add(groupButton);

            rootVisualElement.Add(toolbar);
        }

        private void RequestDataOperation(bool save)
        {
            if (string.IsNullOrEmpty(m_FileName))
            {
                EditorUtility.DisplayDialog("Invalid file name", "Enter a valid file name.", "OK");
                return;
            }

            var saveUtility = VJGraphSaveUtility.GetInstance(m_GraphView);

            if (save)
                saveUtility.SaveGraph(m_FileName);
            else
                saveUtility.LoadGraph(m_FileName);
        }

        private void OnDisable()
        {
            if (m_GraphView != null)
                rootVisualElement.Remove(m_GraphView);
        }
    }
}
