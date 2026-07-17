using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cadi.Shaders.UI.BlurKit
{
    public sealed class Blur : MonoBehaviour
    {
        [SerializeField]
        private Material m_BlurMaterial;

        [SerializeField]
        private Camera m_BlurCamera;

        [SerializeField]
        private RawImage m_RawImage;

        [SerializeField, Range(0f, 1f)]
        private float m_BlurAmount = 0.2f;

        [SerializeField]
        private List<Canvas> m_BackgroundCanvases = new List<Canvas>();

        private List<RenderMode> m_BackgroundCanvasRenderModes = new List<RenderMode>();

        private RenderTexture m_CaptureTexture;
        private RenderTexture m_BlurredTexture;
        private const float c_RenderScale = 1f;

        private bool m_Enabled;

        private static readonly int s_SBlurSize = Shader.PropertyToID("_BlurSize");

        public RenderTexture BlurredTexture => m_BlurredTexture;

        private void Awake()
        {
            m_BlurCamera ??= GetComponentInChildren<Camera>();
            m_RawImage ??= GetComponentInChildren<RawImage>();
            
            CreateTextures();
        }

        private void Start()
        {
            DisableBlur();
        }

        private void OnDestroy()
        {
            ReleaseTextures();
        }
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void EnableBlur()
        {
            ToggleBlur(true);
        }
        
#if ODIN_INSPECTOR
        [Sirenix.OdinInspector.Button]
#endif
        public void DisableBlur()
        {
            ToggleBlur(false);
        }

        public void ToggleBlur(bool enable)
        {
            var bgCanvases = m_BackgroundCanvases;

            if (enable && !m_Enabled)
            {
                m_Enabled = true;
                m_BackgroundCanvasRenderModes.Clear();
                foreach (var canvas in bgCanvases)
                {
                    m_BackgroundCanvasRenderModes.Add(canvas.renderMode);
                    canvas.renderMode = RenderMode.ScreenSpaceCamera;
                }
            }
            else if(!enable && m_Enabled)
            {
                m_Enabled = false;
                for (var i = 0; i < m_BackgroundCanvasRenderModes.Count; i++)
                {
                    bgCanvases[i].renderMode = m_BackgroundCanvasRenderModes[i];
                }
            }

            UpdateBlurTexture();

            m_RawImage.enabled = enable;
        }

        private void UpdateBlurTexture()
        {
            if (m_BlurCamera == null || m_BlurMaterial == null)
            {
                Debug.LogWarning("Cam or Mat is not assigned.");
                return;
            }

            if (m_CaptureTexture == null || m_BlurredTexture == null)
                CreateTextures();

            var previousTarget = m_BlurCamera.targetTexture;
            var previousActive = RenderTexture.active;

            m_BlurCamera.targetTexture = m_CaptureTexture;
            m_BlurCamera.Render();

            ApplyBlur(m_CaptureTexture, m_BlurredTexture);

            m_BlurCamera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
        }

        private void CreateTextures()
        {
            ReleaseTextures();

            int width = Mathf.Max(1, Mathf.RoundToInt(Screen.width * c_RenderScale));
            int height = Mathf.Max(1, Mathf.RoundToInt(Screen.height * c_RenderScale));

            m_CaptureTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "UIBlur_Capture",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
            };
            m_CaptureTexture.Create();

            m_BlurredTexture = new RenderTexture(width, height, 24, RenderTextureFormat.ARGB32)
            {
                name = "UIBlur_Blurred",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            m_BlurredTexture.Create();

            m_RawImage.texture = m_BlurredTexture;
        }

        private void ApplyBlur(RenderTexture source, RenderTexture destination)
        {
            RenderTexture tempA = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);
            RenderTexture tempB = RenderTexture.GetTemporary(source.width, source.height, 0, source.format);

            Graphics.Blit(source, tempA);

            var it = Mathf.FloorToInt(Mathf.Lerp(1f, 4f, m_BlurAmount));
            var blurRad = Mathf.Lerp(0.23f, 8f, m_BlurAmount);

            for (int i = 0; i < it; i++)
            {
                float radius = blurRad + i;

                m_BlurMaterial.SetVector(s_SBlurSize, new Vector4(radius, 0f, 0f, 0f));
                Graphics.Blit(tempA, tempB, m_BlurMaterial, 0);

                m_BlurMaterial.SetVector(s_SBlurSize, new Vector4(0f, radius, 0f, 0f));
                Graphics.Blit(tempB, tempA, m_BlurMaterial, 0);
            }

            Graphics.Blit(tempA, destination);

            RenderTexture.ReleaseTemporary(tempA);
            RenderTexture.ReleaseTemporary(tempB);
        }

        private void ReleaseTextures()
        {
            if (m_CaptureTexture != null)
            {
                m_CaptureTexture.Release();
                Destroy(m_CaptureTexture);
                m_CaptureTexture = null;
            }

            if (m_BlurredTexture != null)
            {
                m_BlurredTexture.Release();
                Destroy(m_BlurredTexture);
                m_BlurredTexture = null;
            }
        }
    }
}