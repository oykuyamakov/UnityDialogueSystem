using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    [ResultInfo("Sound", "Audio")]
    public sealed class SoundResult : Result
    {
        [SerializeField]
        private float m_Volume = 1f;

        public float Volume
        {
            get => m_Volume;
            set => m_Volume = value;
        }

        public SoundResult() { }

        public SoundResult(float volume)
        {
            m_Volume = volume;
        }

        public override Result Clone()
        {
            return new SoundResult(m_Volume);
        }
    }
}
