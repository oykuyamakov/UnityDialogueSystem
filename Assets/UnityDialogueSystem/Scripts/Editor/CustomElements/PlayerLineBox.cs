using UnityDialogueSystem.Scripts.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityDialogueSystem.Scripts.Editor.CustomElements
{
    public sealed class PlayerLineBox : LineBox
    {
        private PlayerLine m_PlayerLine;
        private PlayerNode m_PlayerNode;

        public Port AnswerPort => m_BaseContent.Q<Port>();

        public PlayerLineBox(PlayerLine line, PlayerNode node)
        {
            m_PlayerNode = node;
            m_PlayerLine = line;

            InitiateBox(new Color(0.17f, 0.47f, 0.42f, 0.72f));
        }

        protected override void AddTextBox()
        {
            int outputPortCount = m_PlayerNode.GetLines().Count;
            string answerText = string.IsNullOrEmpty(m_PlayerLine.DialogueLine)
                ? $"Choice {outputPortCount + 1}"
                : m_PlayerLine.DialogueLine;

            Port newAnswerPort = m_PlayerNode.GeneratePort(Direction.Output, m_PlayerLine.DialogueLine);
            newAnswerPort.portName = m_PlayerLine.TargetNodeGuid;

            Label oldLabel = newAnswerPort.contentContainer.Q<Label>("type");
            newAnswerPort.contentContainer.Remove(oldLabel);

            TextField textField = new() { name = string.Empty, value = answerText };
            textField.RegisterValueChangedCallback(evt =>
            {
                float textWidth = evt.newValue.Length * 4;
                textField.style.width = Mathf.Clamp(textWidth, 100, 500);

                float lineCount = evt.newValue.Split('\n').Length;
                float textHeight = lineCount * 20;
                textField.style.height = Mathf.Clamp(textHeight, 40, 200);
                newAnswerPort.style.height = Mathf.Clamp(textHeight, 40, 200);

                m_PlayerLine.DialogueLine = evt.newValue;
            });

            textField.style.width = 250;
            textField.style.minHeight = 20;
            textField.style.maxHeight = 200;
            textField.multiline = true;
            textField.style.alignSelf = Align.FlexStart;
            textField.style.whiteSpace = WhiteSpace.Normal;
            textField.AddToClassList("custom-textfield");

            newAnswerPort.contentContainer.Add(new Label(" "));

            Button deleteButton = new(() => m_PlayerNode.RemoveLineBox(this))
            {
                text = "X"
            };

            m_BaseContent.Add(GetDetailsToggle());
            m_BaseContent.Add(textField);
            m_BaseContent.Add(deleteButton);
            m_BaseContent.Add(newAnswerPort);

            m_PlayerNode.RefreshPorts();
            m_PlayerNode.RefreshExpandedState();
        }

        public override Line GetLine()
        {
            return m_PlayerLine;
        }

        public override DialogueNode GetNode()
        {
            return m_PlayerNode;
        }

        protected override bool HasSpecialReq()
        {
            return false;
        }
    }
}