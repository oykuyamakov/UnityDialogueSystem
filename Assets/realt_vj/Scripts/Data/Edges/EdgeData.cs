using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public class EdgeData
    {
        [SerializeField]
        private string m_SourceGuid;

        [SerializeField]
        private string m_TargetGuid;

        [SerializeField]
        private EdgeType m_EdgeType;

        [SerializeField]
        private float m_Delay;

        public string SourceGuid
        {
            get => m_SourceGuid;
            set => m_SourceGuid = value;
        }

        public string TargetGuid
        {
            get => m_TargetGuid;
            set => m_TargetGuid = value;
        }

        public EdgeType EdgeType
        {
            get => m_EdgeType;
            set => m_EdgeType = value;
        }

        public float Delay
        {
            get => m_Delay;
            set => m_Delay = value;
        }

        public EdgeData() { }

        public EdgeData(string sourceGuid, string targetGuid, EdgeType edgeType, float delay = 0f)
        {
            m_SourceGuid = sourceGuid;
            m_TargetGuid = targetGuid;
            m_EdgeType = edgeType;
            m_Delay = delay;
        }
    }
}
