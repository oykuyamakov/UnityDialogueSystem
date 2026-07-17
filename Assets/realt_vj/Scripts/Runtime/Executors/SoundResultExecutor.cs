using RealtVJ.Data;
using UnityEngine;

namespace RealtVJ.Runtime
{
    public class SoundResultExecutor : ResultExecutor<SoundResult>
    {
        [SerializeField]
        private AudioSource m_AudioSource;

        [SerializeField]
        private AudioClip m_Clip;

        protected override void Execute(SoundResult data)
        {
            if (m_AudioSource == null || m_Clip == null) return;
            m_AudioSource.PlayOneShot(m_Clip, data.Volume);
        }

        public override void Revert()
        {
            if (m_AudioSource != null)
                m_AudioSource.Stop();
        }
    }
}
