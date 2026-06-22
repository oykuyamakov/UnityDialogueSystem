using System;
using Newtonsoft.Json;
using UnityDialogueSystem.Scripts.Actions;
using UnityEngine;

namespace UnityDialogueSystem.Scripts.Data
{
    [Serializable]
    public class StartNodeData : NodeData
    {
        [SerializeField]
        public ConditionSet Conditions;

        [SerializeField]
        [HideInInspector]
        public string TargetNodeGuid;

        [NonSerialized]
        public bool HasPerformed = false;

        private bool HasCondition()
        {
            return Conditions != null && Conditions.IsInitializedForDebug();
        }

        public StartNodeData(string guid, Vector2 graphPosition, string targetNodeGuid, ConditionSet conditions)
            : base(guid, graphPosition.x, graphPosition.y)
        {
            TargetNodeGuid = targetNodeGuid;
            Conditions = conditions;
        }

        [JsonConstructor]
        public StartNodeData(string guid, float graphX, float graphY, string targetNodeGuid, ConditionSet conditions)
            : base(guid, graphX, graphY)
        {
            TargetNodeGuid = targetNodeGuid;
            Conditions = conditions;
        }
    }
}