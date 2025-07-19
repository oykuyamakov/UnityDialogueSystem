using DialogueManagement.Actions;
using DialogueManagement.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueManagement.Editor
{
    public class DialogueGraph : EditorWindow
    {
        private DialogueGraphView m_GraphView;
        private NpcName m_NpcName
        {
            get => m_GraphView.OwnerNpc;
            set => m_GraphView.OwnerNpc = value;
        }
        
        private string m_BindedFileNameValue => m_NpcName.ToString() + "/" + m_NpcName.ToString() + "_" + m_UserDefinedFileNameExtension;

        private string m_UserDefinedFileNameExtension ="";
        
        private TextField m_FileNameTextField;
        
        [MenuItem("Graph/Dialogue Graph")]
        public static void OpenDialogueGraphWindow()
        {
            var window = GetWindow<DialogueGraph>();
            window.titleContent = new GUIContent("Dialogue Graph");
        }

        private void OnEnable()
        {
            ConstructGraphView();
            GenerateToolbar();
            
            if (Resources.Load<DialogueContainer>($"Dialogues/Recovery"))
            {
                // if (EditorUtility.DisplayDialog("Recovery", "Do you want to recover your last session?", "Yes <3 ", "No"))
                // {
                    RequestDataOperation(false, m_GraphView, true);
                //}
            }

        }

        private void ConstructGraphView()
        {
            m_GraphView = new DialogueGraphView(this)
            {
                name = "Dialogue Graph"
            };
            
            m_GraphView.StretchToParentSize();
            rootVisualElement.Add(m_GraphView);
        }
        
        private void GenerateToolbar()
        {
            var toolbar = new Toolbar();
            
            var labelEnum = new Label("File Name : ");
            labelEnum.style.alignSelf = Align.FlexStart;
            labelEnum.style.width = 90;
            labelEnum.style.marginLeft = 20;
            labelEnum.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolbar.Add(labelEnum);
           
            var enumFieldControl = new EnumField( NpcName.Any);
            enumFieldControl.RegisterValueChangedCallback(evt =>
            {
                m_NpcName = (NpcName)evt.newValue;
                m_GraphView.UpdateNodeOwner(m_NpcName);
            });
            
            enumFieldControl.style.alignSelf = Align.FlexStart;
            enumFieldControl.style.width = 200;
            toolbar.Add(enumFieldControl);
            
            var fileNameLabel = new Label("_");
            fileNameLabel.style.alignSelf = Align.FlexStart;
            fileNameLabel.style.unityFontStyleAndWeight = FontStyle.Bold;
            fileNameLabel.style.width = 10;
            toolbar.Add(fileNameLabel);

            m_FileNameTextField = new TextField();
            m_FileNameTextField.MarkDirtyRepaint();
            m_FileNameTextField.RegisterValueChangedCallback(evt =>
            {
                m_UserDefinedFileNameExtension = m_FileNameTextField.value;
            });
            toolbar.Add(m_FileNameTextField);
            m_FileNameTextField.style.alignSelf = Align.FlexStart;
            m_FileNameTextField.style.width = 200;
            
            var saveButton = new Button(() => RequestDataOperation(true));
            saveButton.text = "Save";
            toolbar.Add(saveButton);

            var loadButton = new Button(() => RequestDataOperation(false));
            loadButton.text = "Load";
            toolbar.Add(loadButton);
            
            var miniMapToggle = new Toggle("Minimap");
            miniMapToggle.RegisterValueChangedCallback(evt =>
            {
                if (evt.newValue)
                    m_GraphView.InitializeMiniMap();
                else
                    m_GraphView.RemoveMiniMap();
            });
            miniMapToggle.style.marginLeft = 600;
            miniMapToggle.labelElement.style.unityFontStyleAndWeight = FontStyle.Bold;
            toolbar.Add(miniMapToggle);
            
            rootVisualElement.Add(toolbar);
        }
        
        private bool RequestDataOperation(bool save, DialogueGraphView graphView = null, bool recovery = false)
        {
            var graphV = graphView ?? m_GraphView;
            var fileName = recovery ? "Recovery" : m_BindedFileNameValue;
            
            if (string.IsNullOrEmpty(fileName))
            {
                EditorUtility.DisplayDialog("Invalid file name!", "enter a valid name bruhhh", "OkAyY");
                return false;
            }
            
            Debug.Log(fileName);
            
            var saveUtility = GraphSaveUtility.GetInstance(graphV);
            if (save)
            {
                saveUtility.SaveGraph(fileName, recovery);
            }
            else
            {
                saveUtility.LoadGraph(fileName);
            }

            return true;

        }

        private void OnDisable()
        {
            if (m_GraphView != null)
            {
                var graphV = m_GraphView;
                RequestDataOperation(true, graphV, true);
            }
            rootVisualElement.Remove(m_GraphView);
        }
    }
}
