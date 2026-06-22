using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using UnityDialogueSystem.Scripts.Actions;
using UnityEngine;

namespace UnityDialogueSystem.Scripts.Data
{
    [Serializable]
    public class NpcNodeData : NodeData
    {
        [SerializeField]
        [HideInInspector]
        private NpcName m_OwnerNpc;

        [SerializeField]
        private NpcNodeExecutionMode m_ExecutionMode = NpcNodeExecutionMode.Single;

        [SerializeField]
        protected List<NpcLine> m_Lines = new();

        [SerializeField]
        [HideInInspector]
        public string TargetNodeGuid;

        public NpcName OwnerNpc => m_OwnerNpc;

        public NpcNodeExecutionMode ExecutionMode
        {
            get => m_ExecutionMode;
            set => m_ExecutionMode = value;
        }

        public List<NpcLine> Lines
        {
            get => m_Lines;
            set => m_Lines = value;
        }

        public bool HasNextNode => !string.IsNullOrEmpty(TargetNodeGuid);

        public int UnconditionalLineCount => m_Lines?.Count(line => line != null && line.IsUnconditional()) ?? 0;

        public int ConditionalLineCount => m_Lines?.Count(line => line != null && line.IsConditional()) ?? 0;

        public NpcNodeData(
            string guid,
            Vector2 graphPosition,
            string targetNodeGuid,
            List<NpcLine> lines,
            NpcName ownerNpc,
            NpcNodeExecutionMode executionMode = NpcNodeExecutionMode.Single)
            : base(guid, graphPosition)
        {
            m_Lines = lines ?? new List<NpcLine>();
            TargetNodeGuid = targetNodeGuid;
            m_OwnerNpc = ownerNpc;
            m_ExecutionMode = executionMode;
        }

        [JsonConstructor]
        public NpcNodeData(
            string guid,
            float graphX,
            float graphY,
            string targetNodeGuid,
            List<NpcLine> lines,
            NpcName ownerNpc,
            NpcNodeExecutionMode executionMode = NpcNodeExecutionMode.Single)
            : base(guid, graphX, graphY)
        {
            m_Lines = lines ?? new List<NpcLine>();
            TargetNodeGuid = targetNodeGuid;
            m_OwnerNpc = ownerNpc;
            m_ExecutionMode = executionMode;
        }

        public bool GetConditionsOfLine(out ConditionSet conditions, int index = 0)
        {
            if (m_Lines == null || index < 0 || index >= m_Lines.Count)
            {
                conditions = null;
                return false;
            }

            conditions = m_Lines[index].GetConditions(Guid);
            return conditions is { IsSatisfied: false };
        }

        public bool TryGetLineTriggers(int index, out List<Olay> triggers)
        {
            triggers = null;

            if (m_Lines == null || index < 0 || index >= m_Lines.Count)
            {
                return false;
            }

            return m_Lines[index].TryGetTriggers(out triggers);
        }

        public bool TryGetDefaultLine(out NpcLine line)
        {
            line = m_Lines?.FirstOrDefault(t => t != null && t.IsUnconditional());
            return line != null;
        }

        public bool TryGetHighestPrioritySatisfiedConditionalLine(out NpcLine line)
        {
            line = null;

            if (m_Lines == null || m_Lines.Count == 0)
            {
                return false;
            }

            line = m_Lines
                .Where(t => t != null && t.IsConditional() && t.IsSatisfied())
                .OrderByDescending(t => t.Priority)
                .ThenBy(t => m_Lines.IndexOf(t))
                .FirstOrDefault();

            return line != null;
        }

        public bool TryGetSingleLineToPerform(out NpcLine line)
        {
            if (TryGetHighestPrioritySatisfiedConditionalLine(out line))
            {
                return true;
            }

            return TryGetDefaultLine(out line);
        }

        public bool TryGetAllPerformableLines(out List<NpcLine> lines)
        {
            lines = null;

            if (m_Lines == null || m_Lines.Count == 0)
            {
                return false;
            }

            lines = m_Lines
                .Where(t => t != null && t.IsSatisfied())
                .ToList();

            return lines.Count > 0;
        }

        public bool CanAddUnconditionalLine()
        {
            return m_ExecutionMode != NpcNodeExecutionMode.Single || UnconditionalLineCount < 1;
        }

        public bool NewLineShouldStartConditional()
        {
            if (m_ExecutionMode != NpcNodeExecutionMode.Single)
            {
                return false;
            }

            return m_Lines != null && m_Lines.Count > 0;
        }

        public bool RequiresAtLeastOneConditionalLine(bool hasNextNode)
        {
            return m_ExecutionMode == NpcNodeExecutionMode.Repeating && hasNextNode;
        }

        public bool TryValidate(bool hasNextNode, out string errorMessage)
        {
            errorMessage = string.Empty;

            if (m_Lines == null || m_Lines.Count == 0)
            {
                errorMessage = "Npc node must contain at least one line.";
                return false;
            }

            if (m_ExecutionMode == NpcNodeExecutionMode.Single)
            {
                if (UnconditionalLineCount > 1)
                {
                    errorMessage = "Single mode can contain at most one unconditional line.";
                    return false;
                }
            }

            if (m_ExecutionMode == NpcNodeExecutionMode.Repeating)
            {
                if (hasNextNode && ConditionalLineCount < 1)
                {
                    errorMessage = "Repeating mode requires at least one conditioned line when the node has a next node.";
                    return false;
                }
            }

            return true;
        }
    }
    
    public enum NpcNodeExecutionMode
    {
        Single,
        Repeating,
        AllOnce
    }
}