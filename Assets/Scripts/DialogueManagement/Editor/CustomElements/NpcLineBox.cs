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
        
        private VisualElement m_NodeSpecificContent;
        
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
            var animationEnumField = new EnumField("Animation :" ,m_NpcLine.AnimationAction);
            animationEnumField.RegisterValueChangedCallback(evt =>
            {
                m_NpcLine.AnimationAction = (AnimationAction)evt.newValue;
            });
            animationEnumField.style.flexGrow = 200f;
            animationEnumField.style.flexShrink = 100f; 
            
            m_NodeSpecificContent.Add(animationEnumField);
            
            base.AddSettingsBox();
        }

        protected override void InitiateBox(Color color)
        {
            m_NodeSpecificContent = new VisualElement();
            m_NodeSpecificContent.style.flexDirection = FlexDirection.Column;
            
            base.InitiateBox(color);
            
            var deleteButton = new Button(() => { GetNode().RemoveLineBox(this); });
            deleteButton.text = "X";
            deleteButton.style.alignItems = Align.Center;
            
            m_BaseContent.Add(deleteButton);
            
            m_NodeSpecificContent.style.backgroundColor = color;
            m_NodeSpecificContent.style.borderTopWidth = 5f;
            m_NodeSpecificContent.style.borderBottomWidth = 5f;
            m_NodeSpecificContent.style.borderLeftWidth = 5f;
            m_NodeSpecificContent.style.borderRightWidth = 5f;
            m_NodeSpecificContent.style.borderTopColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_NodeSpecificContent.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_NodeSpecificContent.style.borderLeftColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_NodeSpecificContent.style.borderRightColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_NodeSpecificContent.style.paddingBottom = 5;
            m_AllContent.Add(m_NodeSpecificContent);
        }
    }
}
