using System;
using DialogueManagement.Actions;
using Newtonsoft.Json;
//using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace DialogueManagement.Data
{
    [Serializable]
    public class StartNodeData : NodeData
    {
        [SerializeField]
        //[InlineProperty][ShowIf(nameof(HasLineRequirements), true)][HideLabel]
        public DialogueConditionContainer DialogueConditionContainer;
        
        [SerializeField][HideInInspector]
        public string TargetNodeGuid;

        [NonSerialized]
        public bool HasPerformed = false;
        
        private bool HasLineRequirements()
        {
            return DialogueConditionContainer != null && DialogueConditionContainer.DebugIsInitialized();
        }

        public StartNodeData(string guid, Vector2 graphPosition, string targetNodeGuid,
            DialogueConditionContainer dialogueConditions) : base(guid, graphPosition.x, graphPosition.y)
        {
            TargetNodeGuid = targetNodeGuid;
            DialogueConditionContainer = dialogueConditions;
        }
        
        [JsonConstructor]
        public StartNodeData(string guid, float graphX, float graphY, string targetNodeGuid, 
            DialogueConditionContainer dialogueConditions) : base(guid, graphX, graphY)
        {
            DialogueConditionContainer = dialogueConditions;
        }
    }
}