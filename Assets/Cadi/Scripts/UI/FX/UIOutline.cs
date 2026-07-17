using Cadi.Scripts.CacherSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Cadi.Scripts.UI.FX
{
    [ExecuteAlways]
    [DisallowMultipleComponent]
    public sealed class UIOutline : CacherMonoBehaviour
    {
        [SerializeField]
        private Color m_OutlineColor = Color.white;

        [SerializeField, Range(0f, 30f)]
        private float m_OutlineWidthPx = 2f;

        [SerializeField
         //, ShowIf(nameof(m_Inward), false)
         ,Range(0f, 1f)]
        private float m_AlphaThreshold = 0.99f;
        
        [SerializeField
         //, ShowIf(nameof(m_Inward), true)
         , Range(0f, 1f)]
        private float m_InwardBlend = 1f;

        [SerializeField]
        private bool m_Inward = false;

        [SerializeField]
        private Shader m_OutlineShader;

        [SerializeField][HideInInspector][CachedField]
        private Graphic m_Graphic;
        
        [SerializeField]
        private Material m_MatInstance;

        private static readonly int s_OutlineColorId = Shader.PropertyToID("_OutlineColor");
        private static readonly int s_OutlineWidthId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int s_AlphaThresholdId = Shader.PropertyToID("_AlphaThreshold");
        private static readonly int s_InwardBlend = Shader.PropertyToID("_OutlineBlend");
        private static readonly int s_OutlineMode = Shader.PropertyToID("_OutlineMode");

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            
            //Shader.Find can return null in builds if the shader is stripped / not included. If that happens, outline silently does nothing.
            // if not I already did it put "UI/AlphaOutline" in Always Included Shaders.
            if (m_OutlineShader == null)
                m_OutlineShader = Shader.Find("UI/AlphaOutline");

            if (m_OutlineShader == null || m_Graphic == null)
                return;

            if (m_MatInstance == null || m_MatInstance.shader != m_OutlineShader)
            {
                if (m_MatInstance != null)
                    SafeDestroy(m_MatInstance);

                m_MatInstance = new Material(m_OutlineShader) { name = "UI_AlphaOutline (Instance)" };
            }

            m_Graphic.material = m_MatInstance;
            
            Apply();
        }

        private void OnDestroy()
        {
            if (m_Graphic != null && m_Graphic.material == m_MatInstance)
                m_Graphic.material = null;

            if (m_MatInstance != null)
            {
#if UNITY_EDITOR
                SafeDestroy(m_MatInstance);
#else
            Destroy(m_MatInstance);
#endif
                m_MatInstance = null;
            }
        }
        
        private static void SafeDestroy(Object obj)
        {
            if (obj == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
                DestroyImmediate(obj);
            else
                Destroy(obj);
#else
    Destroy(obj);
#endif
        }
        
        private void Apply()
        {
            if (m_MatInstance == null)
                return;

            m_MatInstance.SetFloat( s_OutlineMode, m_Inward ? 1f : 0f);
            m_MatInstance.SetFloat(s_InwardBlend, m_InwardBlend);
            m_MatInstance.SetColor(s_OutlineColorId, m_OutlineColor);
            m_MatInstance.SetFloat(s_OutlineWidthId, m_OutlineWidthPx);
            m_MatInstance.SetFloat(s_AlphaThresholdId, m_AlphaThreshold);
        }
        
        
        public void SetOutlineColor(Color color, float width = 8f)
        {
            m_OutlineColor = color;
            m_OutlineWidthPx = width;
            Apply();
        }
    }
}