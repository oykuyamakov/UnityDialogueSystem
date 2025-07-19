using DialogueManagement.Actions;
using DialogueManagement.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueManagement.Editor.CustomElements
{
    public sealed class NpcLineBox : LineBox
    { 
        private NpcLine m_NpcLine;
        private NpcNode NpcNode;
        
        public override Line GetLine()
        {
            return m_NpcLine;
        }

        public override DialogueNode GetNode()
        {
            return NpcNode;
        }
        
        public NpcLineBox(NpcNode node,NpcLine npcLine)
        {
            m_NpcLine = npcLine;
            NpcNode = node;

            InitiateBox(new Color(0.43f, 0.22f, 0.47f, 0.72f));
        }

        protected override bool HasSpecialReq()
        {
            return true;
        }

        protected override void AddSettingsBox()
        {
            var animationEnumField = new EnumField(m_NpcLine.AnimationAction);
            animationEnumField.RegisterValueChangedCallback(evt =>
            {
                m_NpcLine.AnimationAction = (AnimationAction)evt.newValue;
            });
            animationEnumField.style.flexGrow = 1f;
            animationEnumField.style.flexShrink = 1f; 
            
            m_NodeSpecificContent.Add(animationEnumField);
            
            base.AddSettingsBox();
        }

        protected override void InitiateBox(Color color)
        {
            base.InitiateBox(color);
            
            var deleteButton = new Button(() => { GetNode().RemoveLineBox(this); });
            deleteButton.text = "X";
            deleteButton.style.alignItems = Align.Center;
            
            m_BaseContent.Add(deleteButton);
        }
    }
}
