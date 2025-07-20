using System.Collections.Generic;
using DialogueManagement.Actions;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueManagement.Editor.CustomElements
{
    public class TriggerBox : Box
    {
        private DialogueNode m_Node;
        public List<DialogueTrigger> TriggerActions => m_TriggerActions;
        private List<DialogueTrigger> m_TriggerActions;
        
        private VisualElement m_TriggerBoxArea;
        
        private VisualElement m_AllTriggers;
        private VisualElement m_TitleArea;
        
        private Color m_FrameColor = new Color(0.1f, 0.36f, 0.25f);
        private Color m_BackgroundColor = new Color(0.06f, 0.13f, 0.06f, 0.85f);
        private Color m_TextHeaderColor = new Color(0.3f, 0.43f, 0.35f);
        
        public TriggerBox(DialogueNode node, List<DialogueTrigger> triggerActions)
        {
            m_Node = node;
            m_TriggerActions = triggerActions;
            
            InitiateBox();
            
            m_Node.extensionContainer.Add(this);

            m_Node.RefreshExpandedState();
            m_Node.RefreshPorts();
        } 
        
        private void InitiateBox()
        {
            m_AllTriggers = new VisualElement();
            m_AllTriggers.style.flexDirection = FlexDirection.Column;
            m_AllTriggers.style.alignItems = Align.Center;
            
            m_TitleArea = new VisualElement();
            m_TitleArea.style.flexDirection = FlexDirection.Row;
            m_TitleArea.style.alignItems = Align.Stretch;
            
            m_AllTriggers.Add(m_TitleArea);
            
            var label = new Label($"  ~ TRIGGERS ~ ");
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize = 10;
            label.style.paddingTop = 5f;
            label.style.paddingBottom = 5f;
            label.style.color = m_TextHeaderColor;
            m_TitleArea.Add(label);
            
            var button = new Button(() =>
            {
                var newTrigger = new DialogueTrigger(new PlayerAction());
                m_TriggerActions.Add(newTrigger);
                CreateTrigger(newTrigger);
            });
            button.text = " + ";
            button.style.alignItems = Align.FlexEnd;
            button.style.backgroundColor = m_FrameColor;
            button.DrawFrameAround(new Color(0.09f, 0.08f, 0.11f, 0.79f));
            
            m_TitleArea.Add(button);
            
            foreach (var trigger in m_TriggerActions)
            {
                if(trigger == null) 
                    continue;
                
                CreateTrigger(trigger);
            }
            
            m_AllTriggers.DrawFrameAround(m_FrameColor);
            m_AllTriggers.style.backgroundColor = m_BackgroundColor;
            m_AllTriggers.style.marginTop = 4f;
            m_AllTriggers.style.marginBottom = 4f;
            
            Add(m_AllTriggers);
        }
        
        private void CreateTrigger(DialogueTrigger dialogueTrigger)
        {
            var borderColor = new Color(0.22f, 0.51f, 0.47f, 0.92f);
            var index = m_TriggerActions.IndexOf(dialogueTrigger) + 1;
            
            m_AllTriggers.AddListActionBox(dialogueTrigger, borderColor,index,
                () =>
                {
                    m_AllTriggers.style.display = m_TriggerActions.Count > 0 ? DisplayStyle.Flex : DisplayStyle.None;
                    m_TriggerActions.Remove(dialogueTrigger);
                });
        }
    }
}
