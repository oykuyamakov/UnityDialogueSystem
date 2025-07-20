using DialogueManagement.Actions;
using DialogueManagement.Editor.CustomElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueManagement.Editor
{
    public class StartNode : DialogueNode
    {
        public DialogueConditionContainer DialogueConditionContainer => m_DialogueConditionContainer;
        private DialogueConditionContainer m_DialogueConditionContainer;
        
        private VisualElement m_LineReqField;
        
        public StartNode(string guid, string nodeTitle,DialogueConditionContainer dialogueConditions) : base(guid, nodeTitle)
        {
            m_DialogueConditionContainer = dialogueConditions;
            
            GeneratePort(Direction.Output, "Next", Port.Capacity.Single);
            
            AddLineReqField();
            
            RefreshExpandedState();
            RefreshPorts();
        }
        
        
        private void AddLineReqField()
        {
            m_LineReqField = new VisualElement();
            m_LineReqField.Add(GraphNodeExtensions.GetConditionBox(DialogueConditionContainer, true));
            
            this.contentContainer.style.backgroundColor = new Color(0.05f, 0.51f, 0.1f);
            
            extensionContainer.Add(m_LineReqField);
        }

        public override void RemoveLineBox(LineBox lineBox)
        {
            throw new System.NotImplementedException();
        }
    }
}