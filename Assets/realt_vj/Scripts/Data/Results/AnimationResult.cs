using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    [ResultInfo("Animation", "Scene")]
    public sealed class AnimationResult : Result
    {
        [SerializeField]
        private string m_TriggerName;

        public string TriggerName
        {
            get => m_TriggerName;
            set => m_TriggerName = value;
        }

        public AnimationResult() { }

        public AnimationResult(string triggerName)
        {
            m_TriggerName = triggerName;
        }

        public override Result Clone()
        {
            return new AnimationResult(m_TriggerName);
        }
    }
}
