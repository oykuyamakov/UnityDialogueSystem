using System;
using System.Collections.Generic;
using System.Linq;
//using Sirenix.OdinInspector;
using UnityEngine;

namespace DialogueManagement.Actions
{
    [Serializable]
    public class DialogueConditionContainer
    {
        [SerializeField] 
        public Location Location = Location.Any;

        [SerializeField]
        //[HorizontalGroup("Row")] [GUIColor(0.8f, 0.31f, 0.27f)]
        public bool Persistent = false;

        [SerializeField]
        //[HorizontalGroup("Row")] [GUIColor(0.8f, 0.31f, 0.27f)]
        public bool Cut = false;

        [NonSerialized]
        public string NodeGuid;

        [SerializeField]
        //[InlineProperty] [ListDrawerSettings(DraggableItems = false, DefaultExpandedState = true, ShowFoldout = false)] [GUIColor(0.8f, 0.31f, 0.27f)]
        private List<DialogueCondition> m_Conditions = new();

        private Dictionary<ActionType, List<DialogueCondition>> m_ActionDic = new();
        public int ConditionCount => m_Conditions.Count;

        public bool GetAllTypes(out ActionType types)
        {
            types = 0; // Default type
            if (m_Conditions == null || m_Conditions.Count == 0)
                return false; // Default type

            foreach (var action in m_Conditions)
            {
                if (action.Action == null)
                {
                    Debug.LogError("ActionData has null action, cannot determine type");
                    continue;
                }

                types |= action.Action.Type;
            }
            return true;
        }
        
        private void SetUpDictionary()
        {
            m_ActionDic.Clear();
            m_ActionDic = new Dictionary<ActionType, List<DialogueCondition>>();

            foreach (var action in m_Conditions)
            {
                if (!m_ActionDic.ContainsKey(action.Action.Type))
                {
                    m_ActionDic[action.Action.Type] = new List<DialogueCondition>();
                }

                m_ActionDic[action.Action.Type].Add(action);
            }
        }

        public List<DialogueCondition> Get()
        {
            return m_Conditions;
        }

        public DialogueCondition Add(ActionRef newAction)
        {
            if (newAction == null)
            {
                Debug.LogError("Cannot add null action to ActionContainer");
                return null;
            }

            var actionData = new DialogueCondition(newAction);
            m_Conditions.Add(actionData);
            SetUpDictionary();
            return actionData;
        }

        public void Remove(DialogueCondition dialogueCondition)
        {
            if (m_Conditions == null || !m_Conditions.Contains(dialogueCondition))
                return;
            m_Conditions.Remove(dialogueCondition);
            SetUpDictionary();
        }

        public bool DebugIsInitialized()
        {
            return m_Conditions is { Count: > 0 } || Location != Location.Any;
        }

        public bool IsSatisfied()
        {
            return m_Conditions == null || !m_Conditions.Any() ||
                   m_Conditions.All(requirement => requirement.IsSatisfied());
        }

        public void FullReset()
        {
            foreach (var requirementAction in m_Conditions)
            {
                requirementAction.RefreshForNewGame();
            }

            SetUpDictionary();
        }

        public bool TryCancelSatisfactionOf(ActionRef cancelAction)
        {
            if (m_Conditions == null || m_Conditions.Count == 0)
                return false;

            foreach (var requirement in m_Conditions.Where(requirement =>
                         requirement.IsSatisfied() && requirement.Action.CheckActionMatch(cancelAction)))
            {
                requirement.SetSatisfaction(false);
                return true;
            }

            return false;
        }

        public bool TrySatisfy(ActionRef triggerAction, Location loc)
        {
            if (!GetMatchingActionData(loc, triggerAction, out var matchingActions))
                return false;

            foreach (var requirement in matchingActions.Where(requirement =>
                         requirement.Action.CheckActionMatch(triggerAction)))
            {
                requirement.SetSatisfaction(true);
            }

            return IsSatisfied();
        }

        private bool GetMatchingActionData(Location loc, ActionRef triggerToMatch,
            out List<DialogueCondition> matchingActions)
        {
            matchingActions = null;

            return Location == loc && m_ActionDic.TryGetValue(triggerToMatch.Type, out matchingActions);
        }
    }
}