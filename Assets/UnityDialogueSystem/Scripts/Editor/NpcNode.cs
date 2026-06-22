using System;
using System.Collections.Generic;
using System.Linq;
using UnityDialogueSystem.Scripts.Actions;
using UnityDialogueSystem.Scripts.Data;
using UnityDialogueSystem.Scripts.Editor.CustomElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityDialogueSystem.Scripts.Editor
{
    public class NpcNode : DialogueNode
    {
        public NpcName MainOwner { get; set; }

        public NpcNodeExecutionMode ExecutionMode
        {
            get => m_ExecutionMode;
            set
            {
                m_ExecutionMode = value;
                RefreshValidationState();
            }
        }

        private NpcNodeExecutionMode m_ExecutionMode = NpcNodeExecutionMode.Single;

        private readonly List<NpcLineBox> m_LinesBoxes = new();
        private Label m_ValidationLabel;

        public NpcNode(string guid, string nodeTitle, List<NpcLine> lines, NpcName owner,
            NpcNodeExecutionMode executionMode = NpcNodeExecutionMode.Single)
            : base(guid, nodeTitle)
        {
            MainOwner = owner;
            m_ExecutionMode = executionMode;

            NodeContentContainer = new VisualElement();
            NodeContentContainer.style.flexDirection = FlexDirection.Column;
            extensionContainer.Add(NodeContentContainer);

            titleContainer.style.paddingBottom = 3f;
            titleContainer.style.paddingTop = 3f;
            titleContainer.style.paddingLeft = 5f;
            titleContainer.style.paddingRight = 5f;
            titleContainer.style.justifyContent = Justify.FlexStart;

            AddNpcNameContainer();
            AddExecutionModeContainer();
            AddNewLineButton();
            AddValidationLabel();

            GeneratePort(Direction.Input, "Prev");
            GeneratePort(Direction.Output, "Next");

            foreach (NpcLine line in lines)
            {
                NpcLineBox newLine = new(this, line);
                m_LinesBoxes.Add(newLine);
            }

            if (lines.Count == 0)
            {
                NpcLineBox introLine = new(this, CreateNewLineData());
                m_LinesBoxes.Add(introLine);
            }

            RefreshValidationState();
            RefreshPorts();
        }

        public List<NpcLine> GetLines()
        {
            List<NpcLine> lines = new();

            foreach (NpcLineBox box in m_LinesBoxes)
            {
                if (box.GetLine() is NpcLine line)
                {
                    lines.Add(line);
                }
            }

            return lines;
        }

        public override void RemoveLineBox(LineBox lineBox)
        {
            if (lineBox is not NpcLineBox npcLineBox)
            {
                throw new InvalidCastException("LineBox is not of type NpcLineBox");
            }

            int index = m_LinesBoxes.IndexOf(npcLineBox);
            if (index < 0)
            {
                return;
            }

            NodeContentContainer.Remove(npcLineBox);
            m_LinesBoxes.RemoveAt(index);

            RefreshValidationState();
            RefreshPorts();
            RefreshExpandedState();
        }

        private void AddNewLineButton()
        {
            Button button = new(() =>
            {
                NpcLineBox newLine = new(this, CreateNewLineData());
                m_LinesBoxes.Add(newLine);

                RefreshValidationState();
            });

            button.text = " + Line";
            button.style.alignSelf = Align.Stretch;
            button.style.backgroundColor = new Color(0.43f, 0.22f, 0.47f, 0.72f);
            button.DrawFrameAround(new Color(0.34f, 0.29f, 0.47f, 0.72f));

            topContainer.Add(button);

            RefreshExpandedState();
            RefreshPorts();
        }

        private NpcLine CreateNewLineData()
        {
            ConditionSet conditions = new();

            if (ShouldCreateConditionalLine())
            {
                conditions.Add(new ItemOlay());
            }

            return new NpcLine("...", conditions, new TriggerSet(), AnimationAction.Default, 0);
        }

        private bool ShouldCreateConditionalLine()
        {
            if (m_ExecutionMode != NpcNodeExecutionMode.Single)
            {
                return false;
            }

            return GetLines().Count > 0;
        }

        private void AddNpcNameContainer()
        {
            EnumField personNameEnumField = new(MainOwner);
            personNameEnumField.RegisterValueChangedCallback(evt => { MainOwner = (NpcName)evt.newValue; });

            personNameEnumField.value = MainOwner;
            personNameEnumField.style.display = DisplayStyle.Flex;
            personNameEnumField.style.width = 100f;

            titleContainer.Add(personNameEnumField);
        }

        private void AddExecutionModeContainer()
        {
            EnumField executionModeField = new(ExecutionMode);
            executionModeField.RegisterValueChangedCallback(evt =>
            {
                ExecutionMode = (NpcNodeExecutionMode)evt.newValue;
            });

            executionModeField.style.display = DisplayStyle.Flex;
            executionModeField.style.width = 170f;

            titleContainer.Add(executionModeField);
        }

        private void AddValidationLabel()
        {
            m_ValidationLabel = new Label();
            m_ValidationLabel.style.whiteSpace = WhiteSpace.Normal;
            m_ValidationLabel.style.fontSize = 10;
            m_ValidationLabel.style.marginTop = 4;
            m_ValidationLabel.style.marginBottom = 4;
            m_ValidationLabel.style.color = new Color(1f, 0.65f, 0.25f);

            extensionContainer.Add(m_ValidationLabel);
        }

        public void RefreshValidationState()
        {
            NpcNodeData tempData = new(
                GUID,
                Vector2.zero,
                OutputPort != null && OutputPort.connections.Any() ? "HAS_NEXT" : string.Empty,
                GetLines(),
                MainOwner,
                ExecutionMode);

            bool hasNextNode = OutputPort != null && OutputPort.connections.Any();

            if (tempData.TryValidate(hasNextNode, out string errorMessage))
            {
                m_ValidationLabel.text = string.Empty;
                m_ValidationLabel.style.display = DisplayStyle.None;
            }
            else
            {
                m_ValidationLabel.text = errorMessage;
                m_ValidationLabel.style.display = DisplayStyle.Flex;
            }

            RefreshExpandedState();
        }
    }
}