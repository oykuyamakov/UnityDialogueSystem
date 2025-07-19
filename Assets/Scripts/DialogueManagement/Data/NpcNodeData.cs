using System;
using System.Collections.Generic;
using System.Linq;
using DialogueManagement.Actions;
using Newtonsoft.Json;
//using Sirenix.OdinInspector;
using UnityEngine;

namespace DialogueManagement.Data
{
    [Serializable]
    public class NpcNodeData : NodeData
    {
        [SerializeField][HideInInspector]
        private NpcName m_OwnerNpc;
        public NpcName OwnerNpc => m_OwnerNpc;
        
        [SerializeField]
        //[ShowInInspector]
        //[InlineProperty][ListDrawerSettings(DraggableItems = false, DefaultExpandedState = true, ShowFoldout = false,HideAddButton = true, HideRemoveButton = true, ListElementLabelName = null, ShowItemCount = false)]
        public List<NpcLine> m_Lines;
        
        [SerializeField][HideInInspector]
        public string TargetNodeGuid;
        
        [SerializeField]
        //[InlineProperty][ListDrawerSettings(DraggableItems = false, DefaultExpandedState = true, ShowFoldout = false,HideAddButton = true, HideRemoveButton = true)][GUIColor(0.38f, 0.65f, 0.57f)]
        public List<DialogueTrigger> Triggers;
        
        private List<ActionRef> m_Triggers
        {
            get
            {
                if (Triggers == null || Triggers.Count == 0)
                    return null;

                return Triggers.Select(trigger => trigger.Action).ToList();
            }
        }
        
        private bool HasTriggers()
        {
            return Triggers != null && Triggers.Count > 0;
        }

        public bool GetConditionsOfLine(out DialogueConditionContainer dialogueConditions, int index = 0)
        {
            if (m_Lines.Count > index)
            {
                dialogueConditions = m_Lines[index].GetDialogueCondition(Guid);
                return dialogueConditions != null && !dialogueConditions.IsSatisfied();
            }
            
            dialogueConditions = null;
            return false;
        }
        
        public bool GetTriggers(out List<ActionRef> triggers)
        {
            triggers = m_Triggers;
            return triggers != null && m_Triggers.Count > 0;
        }
        
        public NpcNodeData(string guid, Vector2 graphPosition, string targetNodeGuid, List<NpcLine> lines,List<DialogueTrigger> triggers,
            NpcName ownerNpc) : base(guid, graphPosition)
        {
            m_Lines = lines;
            
            Triggers = triggers;
            
            TargetNodeGuid = targetNodeGuid;
            
            m_OwnerNpc = ownerNpc;
        }

      
        [JsonConstructor]
        public NpcNodeData(string guid, float graphX, float graphY, string targetNodeGuid, List<NpcLine> lines,List<DialogueTrigger> triggers,
            NpcName ownerNpc) : base(guid, graphX, graphY)
        {
            m_Lines = lines;
            
            Triggers = triggers;
            
            TargetNodeGuid = targetNodeGuid;
            
            m_OwnerNpc = ownerNpc;
        }
        
    }
}
