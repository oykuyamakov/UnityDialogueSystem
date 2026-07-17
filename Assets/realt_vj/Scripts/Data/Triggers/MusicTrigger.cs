using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public sealed class MusicTrigger : Trigger
    {
        [SerializeField]
        private MusicTriggerType m_MusicTriggerType;

        [SerializeField]
        private float m_Value;

        public override TriggerType Type => TriggerType.Music;

        public MusicTriggerType MusicTriggerType
        {
            get => m_MusicTriggerType;
            set => m_MusicTriggerType = value;
        }

        public float Value
        {
            get => m_Value;
            set => m_Value = value;
        }

        public MusicTrigger() { }

        public MusicTrigger(MusicTriggerType musicTriggerType, float value)
        {
            m_MusicTriggerType = musicTriggerType;
            m_Value = value;
        }

        public override Trigger Clone()
        {
            return new MusicTrigger(m_MusicTriggerType, m_Value);
        }
    }
}
