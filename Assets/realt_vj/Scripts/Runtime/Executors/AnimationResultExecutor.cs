using RealtVJ.Data;
using UnityEngine;

namespace RealtVJ.Runtime
{
    public class AnimationResultExecutor : ResultExecutor<AnimationResult>
    {
        [SerializeField]
        private Animator m_Animator;

        protected override void Execute(AnimationResult data)
        {
            if (m_Animator == null || string.IsNullOrEmpty(data.TriggerName)) return;
            m_Animator.SetTrigger(data.TriggerName);
        }
    }
}
