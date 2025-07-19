using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace DialogueManagement.Data
{
    [Serializable]
    public class PlayerNodeData : NodeData
    {
        [SerializeField]
        private List<PlayerLine> m_PlayerLines;
        public List<PlayerLine> PlayerLines
        {
            get => m_PlayerLines;
            set => m_PlayerLines = value;
        }

        public PlayerNodeData(string guid, Vector2 graphPosition, List<PlayerLine> playerLines) : base(guid, graphPosition.x, graphPosition.y)
        {
            m_PlayerLines = playerLines;
        }
        
        [JsonConstructor]
        public PlayerNodeData(string guid, float graphX, float graphY, List<PlayerLine> playerLines) : base(guid, graphX, graphY)
        {
            m_PlayerLines = playerLines;
        }
    }
}
