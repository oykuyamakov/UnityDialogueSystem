using Cadi.Scripts.CacherSystem;
using UnityEngine;

namespace Cadi.Scripts.UI
{
    [RequireComponent(typeof(Camera))]
    public sealed class UICamera : CacherSingleton<UICamera>
    {
        [SerializeField, CachedField]
        private Camera m_Camera;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            SetUp();
        }
        
        //this values can be changed im using the most default values for rendering only the UI,
        //you can change it as per your needs
        private void SetUp()
        {
            // ---------- Projection ----------
            m_Camera.orthographic = true;
            m_Camera.orthographicSize = 1000f;

            // ---------- Clipping ----------
            m_Camera.nearClipPlane = 0.3f;
            m_Camera.farClipPlane  = 1000f;

            // ---------- Rendering ----------
            m_Camera.allowDynamicResolution = false;
            m_Camera.allowMSAA = false; // No Anti-aliasing

            m_Camera.cullingMask = LayerMask.GetMask("UI");

            m_Camera.useOcclusionCulling = true;

            // ---------- Environment ----------
            m_Camera.clearFlags = CameraClearFlags.SolidColor;
            m_Camera.backgroundColor = Color.black;

            // ---------- Output ----------
            m_Camera.targetTexture = null;
            m_Camera.targetDisplay = 0; 
            m_Camera.depth = 0;

            m_Camera.rect = new Rect(0f, 0f, 1f, 1f);

#if UNITY_HDRP
        // ---------- HDRP Additional Camera Data ----------
        var hd = m_Camera.GetComponent<HDAdditionalCameraData>();
        if (hd == null)
            hd = m_Camera.gameObject.AddComponent<HDAdditionalCameraData>();

        hd.customRenderingSettings = false;
        hd.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
        hd.backgroundColorHDR = Color.black;

        hd.volumeLayerMask = ~0; // Everything
        hd.volumeAnchorOverride = null;

        hd.antialiasing = HDAdditionalCameraData.AntialiasingMode.None;
#endif
            
        }

        public Camera Get()
        {
            return m_Camera;
        }
    }
}