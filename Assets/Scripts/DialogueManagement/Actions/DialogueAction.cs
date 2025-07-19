using System;
//using Sirenix.OdinInspector;
using UnityEngine;

namespace DialogueManagement.Actions
{
    public class DialogueAction
    {
        [SerializeReference]
        //[InlineProperty][HideLabel][BoxGroup("ActionBox", ShowLabel = false)] [GUIColor(nameof(GetColor))]
        protected ActionRef m_Action;
        public ActionRef Action
        {
            get => m_Action;
            set => m_Action = value;
        }
        
        public void CopyFrom(DialogueAction from)
        {   
            m_Action = from.m_Action.GetItsClone();
        }
        
        public virtual Color GetColor()
        {
            return new Color(1f, 0.42f, 0.24f);
        }
    }

    [Serializable]
    public class DialogueCondition : DialogueAction
    {
        [NonSerialized] 
        private bool m_Satisfied;

        public bool IsSatisfied()
        {
            return m_Satisfied;
        }
        
        public void SetSatisfaction(bool satisfied)
        {
            m_Satisfied = satisfied;
        }
        
        public DialogueCondition(ActionRef condition)
        {
            m_Action = condition;
        }
        
        public void RefreshForNewGame()
        {
            m_Satisfied = false;
        }
        
        public void CopyFrom(DialogueCondition copyDialogueCondition)
        {   
            m_Action = copyDialogueCondition.m_Action.GetItsClone();
        }
        
        public override Color GetColor()
        {
            return new Color(1f, 0.42f, 0.24f);
        }
    }
    
    
    [Serializable]
    public class DialogueTrigger : DialogueAction
    {
        public DialogueTrigger(ActionRef action)
        {
            Action = action;
        }
        
        public override Color GetColor()
        {
            return new Color(0.37f, 0.99f, 1f);
        }
    }
}