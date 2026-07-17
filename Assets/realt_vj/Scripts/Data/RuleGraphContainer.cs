using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    [CreateAssetMenu(fileName = "NewRuleGraph", menuName = "RealtVJ/Rule Graph Container")]
    public class RuleGraphContainer : ScriptableObject
    {
        [SerializeField]
        public List<TriggerBoxData> TriggerBoxes = new();

        [SerializeField]
        public List<ResultBoxData> ResultBoxes = new();

        [SerializeField]
        public List<DefaultStateBoxData> DefaultStateBoxes = new();

        [SerializeField]
        public List<EdgeData> Edges = new();

        [SerializeField]
        public List<RuleGroupData> RuleGroups = new();

        [NonSerialized]
        private Dictionary<string, BoxData> m_BoxesByGuid;

        [NonSerialized]
        private Dictionary<string, List<EdgeData>> m_EdgesBySourceGuid;

        [NonSerialized]
        private Dictionary<string, List<EdgeData>> m_EdgesByTargetGuid;

        public void InitializeIfRequired()
        {
            if (m_BoxesByGuid != null && m_BoxesByGuid.Count > 0)
                return;

            RebuildCaches();
        }

        public void RebuildCaches()
        {
            m_BoxesByGuid = new Dictionary<string, BoxData>();
            m_EdgesBySourceGuid = new Dictionary<string, List<EdgeData>>();
            m_EdgesByTargetGuid = new Dictionary<string, List<EdgeData>>();

            foreach (var box in TriggerBoxes)
                m_BoxesByGuid[box.Guid] = box;

            foreach (var box in ResultBoxes)
                m_BoxesByGuid[box.Guid] = box;

            foreach (var box in DefaultStateBoxes)
                m_BoxesByGuid[box.Guid] = box;

            foreach (var edge in Edges)
            {
                if (!m_EdgesBySourceGuid.TryGetValue(edge.SourceGuid, out var sourceList))
                {
                    sourceList = new List<EdgeData>();
                    m_EdgesBySourceGuid[edge.SourceGuid] = sourceList;
                }
                sourceList.Add(edge);

                if (!m_EdgesByTargetGuid.TryGetValue(edge.TargetGuid, out var targetList))
                {
                    targetList = new List<EdgeData>();
                    m_EdgesByTargetGuid[edge.TargetGuid] = targetList;
                }
                targetList.Add(edge);
            }
        }

        public bool TryGetBoxByGuid(string guid, out BoxData box)
        {
            InitializeIfRequired();
            return m_BoxesByGuid.TryGetValue(guid, out box);
        }

        public bool TryGetEdgesBySource(string sourceGuid, out List<EdgeData> edges)
        {
            InitializeIfRequired();
            return m_EdgesBySourceGuid.TryGetValue(sourceGuid, out edges);
        }

        public bool TryGetEdgesByTarget(string targetGuid, out List<EdgeData> edges)
        {
            InitializeIfRequired();
            return m_EdgesByTargetGuid.TryGetValue(targetGuid, out edges);
        }
    }
}
