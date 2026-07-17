using Cadi.Scripts.CustomAttributes;
using UnityEngine;

namespace Cadi.Shaders._2D.Frost
{
    public class ShaderFrostController : MonoBehaviour
    {
        [SerializeField]
        private Material m_FrostMaterial;

        [SerializeField]
        [Range(0f, 1f)]
        private float m_TargetGrowth;

        [SerializeField]
        private float m_GrowSpeed = 0.15f;

        [SerializeField]
        private float m_SmoothTime = 0.35f;

        private float m_CurrentGrowth;
        private float m_GrowthVelocity;

        private static readonly int s_GrowthId = Shader.PropertyToID("_Growth");

        private void Awake()
        {
            if (m_FrostMaterial != null)
            {
                m_CurrentGrowth = m_FrostMaterial.GetFloat(s_GrowthId);
            }
        }

        private void Update()
        {
            if (!m_FrostMaterial)
            {
                return;
            }

            float steppedTarget = Mathf.MoveTowards(
                m_CurrentGrowth,
                m_TargetGrowth,
                m_GrowSpeed * Time.deltaTime);

            m_CurrentGrowth = Mathf.SmoothDamp(
                m_CurrentGrowth,
                steppedTarget,
                ref m_GrowthVelocity,
                m_SmoothTime);

            m_FrostMaterial.SetFloat(s_GrowthId, m_CurrentGrowth);
        }

        [Button]
        public void SetTargetGrowth(float value)
        {
            m_TargetGrowth = Mathf.Clamp01(value);
        }

        [Button]
        public void AddGrowth(float delta)
        {
            m_TargetGrowth = Mathf.Clamp01(m_TargetGrowth + delta);
        }

        [Button]
        public void ClearFrost()
        {
            m_TargetGrowth = 0f;
        }

        [Button]
        public void FullFrost()
        {
            m_TargetGrowth = 1f;
        }
    }
}