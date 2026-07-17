using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public class TriggerBoxData : BoxData
    {
        [SerializeReference]
        private List<Trigger> m_Triggers = new();

        [SerializeField]
        private CooldownMode m_CooldownMode = CooldownMode.Full;

        [SerializeField]
        private float m_CooldownDuration;

        public IReadOnlyList<Trigger> Triggers => m_Triggers;

        public CooldownMode CooldownMode
        {
            get => m_CooldownMode;
            set => m_CooldownMode = value;
        }

        public float CooldownDuration
        {
            get => m_CooldownDuration;
            set => m_CooldownDuration = value;
        }

        public TriggerBoxData() { }

        public TriggerBoxData(string guid, Vector2 position) : base(guid, position) { }

        public Trigger AddTrigger(Trigger trigger)
        {
            if (trigger == null) return null;
            m_Triggers.Add(trigger);
            return trigger;
        }

        public bool RemoveTrigger(Trigger trigger)
        {
            if (trigger == null) return false;
            return m_Triggers.Remove(trigger);
        }

        public void RemoveTriggerAt(int index)
        {
            if (index < 0 || index >= m_Triggers.Count) return;
            m_Triggers.RemoveAt(index);
        }

        public void ClearTriggers()
        {
            m_Triggers.Clear();
        }
    }
}
