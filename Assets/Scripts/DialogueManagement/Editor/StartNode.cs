using DialogueManagement.Actions;
using DialogueManagement.Editor.CustomElements;
using UnityEditor.Experimental.GraphView;
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
            
            m_LineReqField = new VisualElement();
            
            AddLineReqField();
            
            RefreshExpandedState();
            RefreshPorts();
        }
        
        
        private void AddLineReqField()
        {
            m_LineReqField.Add(GraphNodeExtensions.GetConditionBox(DialogueConditionContainer, true));
            
            extensionContainer.Add(m_LineReqField);
        }

        public override void RemoveLineBox(LineBox lineBox)
        {
            throw new System.NotImplementedException();
        }
    }
}