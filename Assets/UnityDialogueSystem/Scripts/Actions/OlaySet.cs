using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

namespace UnityDialogueSystem.Scripts.Actions
{
    [Serializable]
    public abstract class OlaySet
    {
        [SerializeReference]
        protected List<Olay> m_Olays = new();

        public IReadOnlyList<Olay> Olays => m_Olays;

        public int Count => m_Olays.Count;

        public bool IsEmpty => m_Olays == null || m_Olays.Count == 0;

        public virtual Color Color => Color.white;

        public List<Olay> Get()
        {
            return m_Olays;
        }

        public virtual Olay Add(Olay newOlay)
        {
            if (newOlay == null)
            {
                Debug.LogError("Cannot add null olay.");
                return null;
            }

            m_Olays.Add(newOlay);
            return newOlay;
        }

        public virtual bool Remove(Olay olay)
        {
            if (olay == null)
            {
                return false;
            }

            if (m_Olays == null || !m_Olays.Contains(olay))
            {
                return false;
            }

            return m_Olays.Remove(olay);
        }

        public virtual void RemoveAt(int index)
        {
            if (m_Olays == null)
            {
                return;
            }

            if (index < 0 || index >= m_Olays.Count)
            {
                return;
            }

            m_Olays.RemoveAt(index);
        }

        public virtual void Clear()
        {
            if (m_Olays == null)
            {
                return;
            }

            m_Olays.Clear();
        }

        public bool ContainsType(OlayType olayType)
        {
            if (m_Olays == null || m_Olays.Count == 0)
            {
                return false;
            }

            return m_Olays.Where(olay => olay != null).Any(olay => olay.Type == olayType);
        }

        public bool TryGetAllTypes(out HashSet<OlayType> types)
        {
            types = null;

            if (m_Olays == null || m_Olays.Count == 0)
            {
                return false;
            }

            types = new HashSet<OlayType>();

            foreach (var olay in m_Olays)
            {
                if (olay == null)
                {
                    Debug.LogError("OlaySet contains null olay.");
                    continue;
                }

                types.Add(olay.Type);
            }

            return types.Count > 0;
        }

        public bool IsInitializedForDebug()
        {
            return m_Olays != null && m_Olays.Count > 0;
        }
    }

    [Serializable]
    public sealed class TriggerSet : OlaySet
    {
        public override Color Color => new(0.37f, 0.99f, 1f);

        public override Olay Add(Olay newOlay)
        {
            return base.Add(newOlay);
        }

        public TriggerSet Clone()
        {
            TriggerSet clone = new TriggerSet();

            if (m_Olays == null || m_Olays.Count == 0)
            {
                return clone;
            }

            foreach (var olay in m_Olays.Where(olay => olay != null))
            {
                clone.Add(olay.Clone());
            }

            return clone;
        }
    }

    [Serializable]
    public sealed class ConditionSet : OlaySet
    {
        [FormerlySerializedAs("LocationType")]
        [SerializeField]
        private Location m_Location = Location.Any;

        [SerializeField]
        private bool m_Persistent = false;

        [SerializeField]
        private bool m_Cut = false;

        [NonSerialized]
        private string m_NodeGuid;

        [NonSerialized]
        private List<bool> m_SatisfiedStates = new();

        [NonSerialized]
        private Dictionary<OlayType, List<int>> m_OlayIndicesByType = new();

        public override Color Color => new(1f, 0.42f, 0.24f);

        public Location Location
        {
            get => m_Location;
            set => m_Location = value;
        }

        public bool Persistent
        {
            get => m_Persistent;
            set => m_Persistent = value;
        }

        public bool Cut
        {
            get => m_Cut;
            set => m_Cut = value;
        }

        public string NodeGuid
        {
            get => m_NodeGuid;
            set => m_NodeGuid = value;
        }

        public int ConditionCount => m_Olays.Count;

        public bool IsSatisfied
        {
            get
            {
                if (m_Olays == null || m_Olays.Count == 0)
                {
                    return true;
                }

                EnsureRuntimeState();

                return m_SatisfiedStates.All(t => t);
            }
        }

        public override Olay Add(Olay newOlay)
        {
            Olay addedOlay = base.Add(newOlay);

            if (addedOlay == null)
            {
                return null;
            }

            RebuildRuntimeCache();
            return addedOlay;
        }

        public override bool Remove(Olay olay)
        {
            bool removed = base.Remove(olay);

            if (!removed)
            {
                return false;
            }

            RebuildRuntimeCache();
            return true;
        }

        public override void RemoveAt(int index)
        {
            base.RemoveAt(index);
            RebuildRuntimeCache();
        }

        public override void Clear()
        {
            base.Clear();
            RebuildRuntimeCache();
        }

        public new bool IsInitializedForDebug()
        {
            return (m_Olays != null && m_Olays.Count > 0) || m_Location != Location.Any;
        }

        public void ResetRuntimeState()
        {
            EnsureRuntimeState();

            for (int i = 0; i < m_SatisfiedStates.Count; i++)
            {
                m_SatisfiedStates[i] = false;
            }
        }

        public void MarkSatisfied(int index, bool satisfied)
        {
            EnsureRuntimeState();

            if (index < 0 || index >= m_SatisfiedStates.Count)
            {
                return;
            }

            m_SatisfiedStates[index] = satisfied;
        }

        public bool GetSatisfaction(int index)
        {
            EnsureRuntimeState();

            if (index < 0 || index >= m_SatisfiedStates.Count)
            {
                return false;
            }

            return m_SatisfiedStates[index];
        }

        public bool TryCancelSatisfactionOf(Olay cancelOlay)
        {
            if (cancelOlay == null)
            {
                return false;
            }

            if (m_Olays == null || m_Olays.Count == 0)
            {
                return false;
            }

            EnsureRuntimeState();

            if (!m_OlayIndicesByType.TryGetValue(cancelOlay.Type, out List<int> matchingIndices))
            {
                return false;
            }

            foreach (var olayIndex in from olayIndex in matchingIndices where m_SatisfiedStates[olayIndex] let olay = m_Olays[olayIndex] where olay != null where olay.Matches(cancelOlay) select olayIndex)
            {
                m_SatisfiedStates[olayIndex] = false;
                return true;
            }

            return false;
        }

        public bool TrySatisfy(Olay incomingOlay, Location currentLocation)
        {
            if (incomingOlay == null)
            {
                return false;
            }

            if (m_Location != currentLocation)
            {
                return false;
            }

            if (m_Olays == null || m_Olays.Count == 0)
            {
                return false;
            }

            EnsureRuntimeState();

            if (!m_OlayIndicesByType.TryGetValue(incomingOlay.Type, out List<int> matchingIndices))
            {
                return false;
            }

            bool anyMatched = false;

            foreach (var olayIndex in from olayIndex in matchingIndices let olay = m_Olays[olayIndex] where olay != null where olay.Matches(incomingOlay) select olayIndex)
            {
                m_SatisfiedStates[olayIndex] = true;
                anyMatched = true;
            }

            return anyMatched && IsSatisfied;
        }

        public bool TryGetMatchingOlays(Olay incomingOlay, Location currentLocation, out List<Olay> matchingOlays)
        {
            matchingOlays = null;

            if (incomingOlay == null)
            {
                return false;
            }

            if (m_Location != currentLocation)
            {
                return false;
            }

            if (m_Olays == null || m_Olays.Count == 0)
            {
                return false;
            }

            EnsureRuntimeState();

            if (!m_OlayIndicesByType.TryGetValue(incomingOlay.Type, out List<int> matchingIndices))
            {
                return false;
            }

            matchingOlays = (from olayIndex in matchingIndices select m_Olays[olayIndex] into olay where olay != null where olay.Matches(incomingOlay) select olay).ToList();

            return matchingOlays.Count > 0;
        }

        public ConditionSet Clone()
        {
            var clone = new ConditionSet
            {
                m_Location = m_Location,
                m_Persistent = m_Persistent,
                m_Cut = m_Cut,
                m_NodeGuid = m_NodeGuid
            };

            if (m_Olays != null && m_Olays.Count > 0)
            {
                for (int i = 0; i < m_Olays.Count; i++)
                {
                    Olay olay = m_Olays[i];

                    if (olay == null)
                    {
                        continue;
                    }

                    clone.m_Olays.Add(olay.Clone());
                }
            }

            clone.RebuildRuntimeCache();
            return clone;
        }

        private void EnsureRuntimeState()
        {
            m_SatisfiedStates ??= new List<bool>();

            m_OlayIndicesByType ??= new Dictionary<OlayType, List<int>>();

            if (m_SatisfiedStates.Count != m_Olays.Count)
            {
                RebuildRuntimeCache();
                return;
            }

            if (m_OlayIndicesByType.Count == 0 && m_Olays.Count > 0)
            {
                RebuildRuntimeCache();
            }
        }

        private void RebuildRuntimeCache()
        {
            m_SatisfiedStates ??= new List<bool>();

            m_OlayIndicesByType ??= new Dictionary<OlayType, List<int>>();

            m_SatisfiedStates.Clear();
            m_OlayIndicesByType.Clear();

            if (m_Olays == null)
            {
                return;
            }

            for (int i = 0; i < m_Olays.Count; i++)
            {
                Olay olay = m_Olays[i];
                m_SatisfiedStates.Add(false);

                if (olay == null)
                {
                    Debug.LogError("ConditionSet contains null olay.");
                    continue;
                }

                if (!m_OlayIndicesByType.TryGetValue(olay.Type, out List<int> indices))
                {
                    indices = new List<int>();
                    m_OlayIndicesByType.Add(olay.Type, indices);
                }

                indices.Add(i);
            }
        }
    }
}