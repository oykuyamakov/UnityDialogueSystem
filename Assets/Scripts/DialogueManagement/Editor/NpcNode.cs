using System.Collections.Generic;
using DialogueManagement.Actions;
using DialogueManagement.Data;
using DialogueManagement.Editor.CustomElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;
namespace DialogueManagement.Editor
{
    public class NpcNode : DialogueNode
    {
        public NpcName MainOwner { get; set; }
        
        private List<NpcLineBox> m_LinesBoxes = new();
        private TriggerBox m_TriggerBox;
        
        public List<NpcLine> GetLines()
        {
            var lines = new List<NpcLine>();
            foreach (var box in m_LinesBoxes)
            {
                lines.Add(box.GetLine() as NpcLine);
            }

            return lines;
        }

        public override void RemoveLineBox(LineBox lineBox)
        {
            if (lineBox is not NpcLineBox npcLineBox)
            {
                throw new System.InvalidCastException("LineBox is not of type NpcLineBox");
            }
            
            var index = m_LinesBoxes.IndexOf(npcLineBox);
            if (index < 0) 
                return;

            NodeContentContainer.Remove(npcLineBox);
            m_LinesBoxes.RemoveAt(index);

            RefreshPorts();
            RefreshExpandedState();
        }

        public NpcNode(string guid, string nodeTitle, List<NpcLine> lines, List<DialogueTrigger> triggers, NpcName owner) : base(
            guid, nodeTitle)
        {
            MainOwner = owner;
            nodeTitle = MainOwner + " Dialogue Node";

            NodeContentContainer = new VisualElement();
            NodeContentContainer.style.flexDirection = FlexDirection.Column;
            
            extensionContainer.Add(NodeContentContainer);
            
            foreach (var line in lines) 
            {
                var newLine = new NpcLineBox(this, line);
                m_LinesBoxes.Add(newLine);
            }
            
            AddNewLineButton();
            
            m_TriggerBox = new TriggerBox(this, triggers);
            
            AddNpcNameContainer();
            
            GeneratePort(Direction.Input, "Prev");
            GeneratePort(Direction.Output, "Next");
            
            RefreshPorts();
        }
        
       
        public List<DialogueTrigger> GetTriggers()
        {
            return m_TriggerBox.TriggerActions;
        }

        private void AddNewLineButton()
        {
            var button = new Button(() =>
            {
                var newLine = new NpcLineBox(this,new NpcLine("...",new DialogueConditionContainer(),AnimationAction.Default));
                m_LinesBoxes.Add(newLine);
            });
            button.text = "New Line";
            
            titleContainer.Add(button);

            RefreshExpandedState();
            RefreshPorts();
        }

        private void AddNpcNameContainer()
        {
            var personBox = GraphNodeExtensions.GetBorderBox(FlexDirection.Row, new Color(0.17f, 0.05f, 0.19f, 0.92f));
            var personNameEnumField = new EnumField(MainOwner);
            personNameEnumField.RegisterValueChangedCallback(evt =>
            {
                MainOwner = (NpcName)evt.newValue;
            });
            personNameEnumField.value = MainOwner;
            personNameEnumField.style.flexGrow = 1f;
            personNameEnumField.style.flexShrink = 1f;
            personNameEnumField.style.display = DisplayStyle.Flex;
            personBox.Add(personNameEnumField);
            titleContainer.Add(personNameEnumField);
        }
    }
}