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
        
        public TriggerBox(DialogueNode node, List<DialogueTrigger> triggerActions)
        {
            m_Node = node;
            
            m_TriggerActions = triggerActions;
            InitiateBox();
            
            var button = new Button(() =>
            {
                var newTrigger = new DialogueTrigger(new PlayerAction());
                m_TriggerActions.Add(newTrigger);
                CreateTrigger(newTrigger);
            });
            button.text = "Add Trigger";
            
            m_Node.titleContainer.Add(button);
            
            m_Node.extensionContainer.Add(this);

            m_Node.RefreshExpandedState();
            m_Node.RefreshPorts();
        } 
        
        private void InitiateBox()
        {
            m_AllTriggers = new VisualElement();
            m_AllTriggers.style.flexDirection = FlexDirection.Column;
            m_AllTriggers.style.alignItems = Align.Center;
            
            foreach (var trigger in m_TriggerActions)
            {
                if(trigger == null) 
                    continue;
                
                CreateTrigger(trigger);
            }
            
            var borderColor = new Color(0.12f, 0.27f, 0.25f, 0.92f);
            
            m_AllTriggers.DrawFrameAround(borderColor);
            
            m_AllTriggers.style.marginLeft = 10f;
            m_AllTriggers.style.marginRight = 10f;
            m_AllTriggers.style.marginTop = 4f;
            m_AllTriggers.style.marginBottom = 4f;
            
            Add(m_AllTriggers);
        }
        
        private void CreateTrigger(DialogueTrigger dialogueCondition)
        {
            var borderColor = new Color(0.12f, 0.27f, 0.25f, 0.92f);
                
            var box = new VisualElement();
            box.style.flexDirection = FlexDirection.Row;

            var index = m_TriggerActions.IndexOf(dialogueCondition);
            
            var label = new Label(" # " + index);
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize = 10;
            label.style.color = new Color(0.98f, 0.98f, 0.98f, 1f);
            label.style.paddingLeft = 5;
            label.style.paddingRight = 5;
            label.style.paddingTop = 5;
            label.style.paddingBottom = 5;
            label.style.backgroundColor = new Color(0.12f, 0.27f, 0.25f, 0.92f);
            
            box.Add(label);
            box.Add(GraphNodeExtensions.GetActionBox(dialogueCondition));
                
            var deleteTrigger = new Button(() => {
            {
                m_TriggerActions.Remove(dialogueCondition);
                m_AllTriggers.Remove(box);
            } });
            deleteTrigger.text = "x";
            deleteTrigger.style.alignItems = Align.Center;

            box.Add(deleteTrigger);

            box.style.borderBottomWidth = 5f;
            box.style.borderTopWidth = 5f;
            box.style.borderTopLeftRadius = 5f;

            box.DrawFrameAround(borderColor);
            
            box.style.marginLeft = 10f;
            box.style.marginRight = 10f;
            box.style.marginTop = 4f;
            box.style.marginBottom = 4f;
            
            m_AllTriggers.style.borderTopWidth = 10f;
            m_AllTriggers.Add(box);
        }
    }
}
