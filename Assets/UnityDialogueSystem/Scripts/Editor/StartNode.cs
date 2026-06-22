using UnityDialogueSystem.Scripts.Actions;
using UnityDialogueSystem.Scripts.Editor.CustomElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityDialogueSystem.Scripts.Editor
{
    public class StartNode : DialogueNode
    {
        public ConditionSet Conditions => m_Conditions;
        private ConditionSet m_Conditions;

        private VisualElement m_LineReqField;

        public StartNode(string guid, string nodeTitle, ConditionSet conditions)
            : base(guid, nodeTitle)
        {
            m_Conditions = conditions;

            GeneratePort(Direction.Output, "Next", Port.Capacity.Single);

            AddLineReqField();

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddLineReqField()
        {
            m_LineReqField = new VisualElement();
            m_LineReqField.Add(GraphNodeExtensions.GetConditionBox(Conditions, true));

            contentContainer.style.backgroundColor = new Color(0.05f, 0.51f, 0.1f);
            extensionContainer.Add(m_LineReqField);
        }

        public override void RemoveLineBox(LineBox lineBox)
        {
            throw new System.NotImplementedException();
        }
    }
}