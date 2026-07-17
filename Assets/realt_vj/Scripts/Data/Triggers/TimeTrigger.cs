using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public sealed class TimeTrigger : Trigger
    {
        [SerializeField]
        private TimeTriggerType m_TimeTriggerType;

        [SerializeField]
        private float m_Duration;

        [SerializeField]
        private int m_RepeatCount = -1;

        public override TriggerType Type => TriggerType.Time;

        public TimeTriggerType TimeTriggerType
        {
            get => m_TimeTriggerType;
            set => m_TimeTriggerType = value;
        }

        public float Duration
        {
            get => m_Duration;
            set => m_Duration = value;
        }

        /// <summary>
        /// -1 = infinite, N = repeat N times.
        /// </summary>
        public int RepeatCount
        {
            get => m_RepeatCount;
            set => m_RepeatCount = value;
        }

        public TimeTrigger() { }

        public TimeTrigger(TimeTriggerType timeTriggerType, float duration, int repeatCount = -1)
        {
            m_TimeTriggerType = timeTriggerType;
            m_Duration = duration;
            m_RepeatCount = repeatCount;
        }

        public override Trigger Clone()
        {
            return new TimeTrigger(m_TimeTriggerType, m_Duration, m_RepeatCount);
        }
    }
}
