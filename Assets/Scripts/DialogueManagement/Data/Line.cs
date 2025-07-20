using System;
using DialogueManagement.Actions;
//using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace DialogueManagement.Data
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

        [HideInInspector] public bool m_ShowSettings = false;

        [FormerlySerializedAs("ConditionActionContainer")]
        [FormerlySerializedAs("ActionContainer")]
        [FormerlySerializedAs("ConditionCollection")]
        [FormerlySerializedAs("ActionSet")]
        [FormerlySerializedAs("DialogueCondition")]
        [FormerlySerializedAs("m_DialogueCondition")]
        [SerializeField]
        //[InlineProperty] [ShowIf(nameof(HasAnyCondition), true)] [HideLabel]
        public DialogueConditionContainer DialogueConditionContainer;
        
        public DialogueConditionContainer GetDialogueCondition(string nodeGuid)
        {
            DialogueConditionContainer.NodeGuid = nodeGuid;
            return DialogueConditionContainer;
        }
        
        public Line(string dialogueLine, DialogueConditionContainer dialogueConditions)
        {
            m_DialogueLine = dialogueLine;
            DialogueConditionContainer = dialogueConditions;
        }

        private bool HasAnyCondition()
        {
            return DialogueConditionContainer != null && DialogueConditionContainer.DebugIsInitialized();
        }

        public void RemoveCondition(DialogueCondition requirement)
        {
            DialogueConditionContainer?.Remove(requirement);
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
        [SerializeField] private AnimationAction m_AnimationAction;

        public AnimationAction AnimationAction
        {
            get => m_AnimationAction;
            set => m_AnimationAction = value;
        }

        public NpcLine(string dialogueLine, DialogueConditionContainer dialogueConditions,
            AnimationAction animationAction)
            : base(dialogueLine, dialogueConditions)
        {
            m_AnimationAction = animationAction;
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

        public PlayerLine(string dialogueLine, string guid, string targetNodeGuid, DialogueConditionContainer dialogueConditions) :
            base(dialogueLine, dialogueConditions)
        {
            TargetNodeGuid = targetNodeGuid;
            Guid = guid;
        }
    }
}