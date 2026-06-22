using System;
using System.Collections.Generic;
using UnityDialogueSystem.Scripts.Actions;
using UnityEngine;

namespace UnityDialogueSystem.Scripts.Data
{
    [Serializable]
    public class Line : ISerializationCallbackReceiver
    {
        [SerializeField]
        private string m_DialogueLine;

        public string DialogueLine
        {
            get => m_DialogueLine;
            set => m_DialogueLine = value;
        }

        [HideInInspector]
        public bool m_ShowSettings = false;

        [SerializeField]
        public ConditionSet Conditions;

        [SerializeField]
        public TriggerSet Triggers;

        public Line(string dialogueLine, ConditionSet conditions, TriggerSet triggers)
        {
            m_DialogueLine = dialogueLine;
            Conditions = conditions;
            Triggers = triggers;
        }

        public ConditionSet GetConditions(string nodeGuid)
        {
            if (Conditions == null)
            {
                return null;
            }

            Conditions.NodeGuid = nodeGuid;
            return Conditions;
        }

        public bool HasAnyCondition()
        {
            return Conditions != null && Conditions.IsInitializedForDebug();
        }

        public bool HasAnyTrigger()
        {
            return Triggers != null && Triggers.IsInitializedForDebug();
        }

        public bool IsConditional()
        {
            return Conditions != null && Conditions.IsInitializedForDebug();
        }

        public bool IsUnconditional()
        {
            return !IsConditional();
        }

        public bool IsSatisfied()
        {
            return Conditions == null || Conditions.IsSatisfied;
        }

        public void RemoveCondition(Olay olay)
        {
            Conditions?.Remove(olay);
        }

        public void RemoveTrigger(Olay olay)
        {
            Triggers?.Remove(olay);
        }

        public bool TryGetTriggers(out List<Olay> triggers)
        {
            triggers = null;

            if (Triggers == null || Triggers.IsEmpty)
            {
                return false;
            }

            triggers = Triggers.Get();
            return triggers != null && triggers.Count > 0;
        }

        public void OnBeforeSerialize()
        {
        }

        public void OnAfterDeserialize()
        {
        }
    }

    [Serializable]
    public class NpcLine : Line
    {
        [SerializeField]
        private AnimationAction m_AnimationAction;

        [SerializeField]
        private int m_Priority;

        public AnimationAction AnimationAction
        {
            get => m_AnimationAction;
            set => m_AnimationAction = value;
        }

        public int Priority
        {
            get => m_Priority;
            set => m_Priority = value;
        }

        public NpcLine(
            string dialogueLine,
            ConditionSet conditions,
            TriggerSet triggers,
            AnimationAction animationAction,
            int priority = 0)
            : base(dialogueLine, conditions, triggers)
        {
            m_AnimationAction = animationAction;
            m_Priority = priority;
        }
    }

    [Serializable]
    public class PlayerLine : Line
    {
        [SerializeField]
        [HideInInspector]
        public string TargetNodeGuid;

        [SerializeField]
        [HideInInspector]
        public string Guid;

        public PlayerLine(string dialogueLine, string guid, string targetNodeGuid, ConditionSet conditions, TriggerSet triggers)
            : base(dialogueLine, conditions, triggers)
        {
            TargetNodeGuid = targetNodeGuid;
            Guid = guid;
        }
    }
}