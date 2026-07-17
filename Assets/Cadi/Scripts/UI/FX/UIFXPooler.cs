using System.Collections.Generic;
using Cadi.Scripts.CacherSystem;
using UnityEngine;
using UnityEngine.UI;

namespace Cadi.Scripts.UI.FX
{
    [RequireComponent(typeof(Canvas))]
    [RequireComponent(typeof(CanvasScaler))]
    public sealed class UIFXPooler : CacherSingleton<UIFXPooler>
    {
        [SerializeField]
        private int m_InitialPoolSize = 10;

        [SerializeField]
        private int m_CanvasSortingOrder = 5000;

        [SerializeField]
        private Vector2 m_ReferenceResolution = new Vector2(1920, 1080);

        [SerializeField, Range(0f, 1f)]
        private float m_MatchWidthOrHeight = 0.5f;

        [SerializeField]
        private RenderMode m_RenderMode = RenderMode.ScreenSpaceOverlay;

      
        private readonly Dictionary<UIFxType, Stack<FXGraphix>> m_FxImgPool = new();

        [CachedField(addComponentIfMissing: true), SerializeField]
        private Canvas m_UICanvas;

        [CachedField(addComponentIfMissing: true), SerializeField]
        private RectTransform m_CanvasRect;

        [CachedField(addComponentIfMissing: true), SerializeField]
        private CanvasScaler m_Scaler;

        private UICamera m_UICamera;
        
        private UIContent m_Content;


        private void Start()
        {
            EnsureCanvas();
            InitPool();
        }

        private void EnsureCanvas()
        {
            m_UICamera = UICamera.Instance;

            m_Content = UIContent.Get();

            if (m_UICanvas == null)
            {
                Debug.LogError(
                    " UIUtility: Canvas component is missing. Please ensure this GameObject has a Canvas component.");
                return;
            }

            m_UICanvas.renderMode = m_RenderMode;

            if (m_RenderMode == RenderMode.ScreenSpaceCamera)
            {
                if (m_UICanvas.worldCamera == null)
                {
                    Debug.LogWarning(
                        "UIUtility: Render mode is set to ScreenSpaceCamera but no camera is assigned. Searching for UI camera with tag 'UICamera Tag");

                    if (m_UICamera != null)
                    {
                        m_UICanvas.worldCamera = m_UICamera.Get();
                        Debug.Log("UIUtility: Assigned camera " + m_UICamera.name + " to Canvas.");
                    }
                    else
                    {
                        Debug.LogError(
                            "UIUtility: No camera found with tag 'UICamera'. Please assign a camera to the Canvas or set the correct render mode.");
                        m_RenderMode = RenderMode.ScreenSpaceOverlay;
                        m_UICanvas.renderMode = m_RenderMode;
                    }
                }
            }

            m_UICanvas.sortingOrder = m_CanvasSortingOrder;

            if (m_CanvasRect == null)
            {
                Debug.LogError(
                    "UIUtility: RectTransform component is missing. Please ensure this GameObject has a RectTransform component.");
            }

            if (m_Scaler == null)
            {
                Debug.LogError(
                    "UIUtility: CanvasScaler component is missing. Please ensure this GameObject has a CanvasScaler component.");
                return;
            }

            m_Scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            m_Scaler.referenceResolution = m_ReferenceResolution;
            m_Scaler.matchWidthOrHeight = m_MatchWidthOrHeight;
            m_Scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        }
        
        
        private void InitPool()
        {
            if(m_Initialized)
                return;
            
            if( m_Content == null)
                m_Content = UIContent.Get();

            m_FxImgPool.Clear();
            var fxPrefabs = m_Content.UiFxContents;
            foreach (var fxP in fxPrefabs)
            {
                if (m_FxImgPool.ContainsKey(fxP.FxType))
                {
                    Debug.LogError("Duplicate UI FX type in Content SO: " + fxP.FxType.ToString());
                    continue;
                }
                
                var stack = ImagePrefabHelpers.InitializeStack(fxP, m_UICanvas.transform, m_InitialPoolSize);
                m_FxImgPool.Add(fxP.FxType, stack);
            }

            m_Initialized = true;
        }
        
        
        private bool m_Initialized = false;

        private FXGraphix GetPooledFxImage(UIFxType type)
        {
            if (m_FxImgPool.TryGetValue(type, out var stack) && stack.Count > 0)
            {
                return stack.Pop();
            }
            else
            {
                if (!m_Initialized)
                {
                    InitPool();
                    return GetPooledFxImage(type);
                }
                else
                {
                    Debug.LogWarning(
                        $"UIUtility: No available pooled image for type {type.ToString()}. Consider increasing the initial pool size.");

                    if (!ImagePrefabHelpers.TryCreate(type, m_UICanvas.transform, out var fx))
                    {
                        Debug.LogError( $"UIUtility: Failed to create FX image for type {type.ToString()}. Please check the prefab assignment in Content SO.");
                    }

                    return fx;
                }
            }
        }

        private void ReturnPooledFxImage(FXGraphix img)
        {
            if (img == null)
                return;

            img.StopFx();
            img.gameObject.SetActive(false);
            img.transform.SetParent(m_UICanvas.transform, false);
            if (!m_FxImgPool.TryGetValue(img.FxType, out var stack))
            {
                stack = new Stack<FXGraphix>();
                m_FxImgPool.Add(img.FxType, stack);
            }

            stack.Push(img);
        }

        // ---------------------------
        // Public API
        // ---------------------------

        /// <summary>
        /// Screen position (px) reveal. Leave Cam null for Overlay canvas.
        /// </summary>
        public void RevealAtScreenPos(UIFxType type, Vector2 screenPos, float duration, Color color, int orderInLayer, Vector2? size = null,
            Camera uiCamera = null)
        {
            m_UICanvas.sortingOrder = orderInLayer;
            
            Vector2 localPos;
            bool ok = RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasRect, screenPos, uiCamera,
                out localPos);
            if (!ok)
            {
                return;
            }

            FinFoutLocalCanvas(orderInLayer,type,localPos, duration, color, size);
        }

        /// <summary>
        /// Target rect transform reveal. Leave Cam bull for Overlay canvas.
        /// </summary>
        public void RevealAtRectT(UIFxType type,RectTransform target, float duration, Color color, int sortingOrder,
            Vector2? size = null, Camera uiCamera = null)
        {
            if (target == null)
            {
                return;
            }

            m_UICanvas.sortingOrder = sortingOrder;

            if (!TryWorldToLocalCanvas(target, uiCamera, out var localPos))
                return;

            FinFoutLocalCanvas(sortingOrder, type, localPos, duration, color, size);
        }


        public void FinAtRect(UIFxType type,RectTransform target, Color color, float duration, int sortingOrder,
            Vector2? size = null, Camera uiCamera = null)
        {
            if (target == null)
            {
                return;
            }

            m_UICanvas.sortingOrder = sortingOrder;

            if (!TryWorldToLocalCanvas(target, uiCamera, out var localPos))
                return;

            FinAtLocalCanvas(sortingOrder,type, localPos, duration, color, size);
        }

        /// <summary>
        /// Spawns a persistent FX at the target rect. Caller owns the returned image
        /// and must call <see cref="HideFxImage"/> to return it to the pool.
        /// </summary>
        public FXGraphix ShowFxAtRect(UIFxType type, RectTransform target, Color color, float duration, int sortingOrder, float sizer,
            Camera uiCamera = null)
        {
            if (target == null)
                return null;

            m_UICanvas.sortingOrder = sortingOrder;

            if (!TryWorldToLocalCanvas(target, uiCamera, out var localPos))
                return null;

            var fxImg = GetPooledFxImage(type);
            var rt = fxImg.CachedRectTransform;

            fxImg.gameObject.SetActive(false);
            rt.SetParent(m_UICanvas.transform, false);
            rt.anchoredPosition = localPos;

            fxImg.Fin(sortingOrder, duration, color);
            fxImg.Sizer(sizer);
            return fxImg;
        }

        public void HideFxImage(FXGraphix image)
        {
            ReturnPooledFxImage(image);
        }

        // ---------------------------
        // Internal helpers
        // ---------------------------

        private static readonly Vector3[] s_Corners = new Vector3[4];

        private bool TryWorldToLocalCanvas(RectTransform target, Camera uiCamera, out Vector2 localPos)
        {
            localPos = default;

            if (uiCamera == null)
                uiCamera = m_RenderMode == RenderMode.ScreenSpaceCamera ? m_UICanvas.worldCamera : null;

            target.GetWorldCorners(s_Corners);

            Vector3 worldCenter = (s_Corners[0] + s_Corners[2]) * 0.5f;
            Vector2 screen = RectTransformUtility.WorldToScreenPoint(uiCamera, worldCenter);

            return RectTransformUtility.ScreenPointToLocalPointInRectangle(m_CanvasRect, screen, uiCamera, out localPos);
        }

        // ---------------------------
        // Internal reveal runner
        // ---------------------------
        private void FinFoutLocalCanvas(int orderInLayer,UIFxType type, Vector2 localCanvasPos, float duration, Color color, Vector2? size)
        {
            if (duration <= 0f)
            {
                duration = 0.01f;
            }

            var uIFxImg = GetPooledFxImage(type);
            var rt = uIFxImg.CachedRectTransform;

            rt.SetParent(m_UICanvas.transform, false);
            rt.anchoredPosition = localCanvasPos;

            if (size.HasValue)
            {
                rt.sizeDelta = size.Value;
            }

            uIFxImg.FinFout( orderInLayer,duration, color, ReturnPooledFxImage);
        }

        private readonly Dictionary<UIFxType, FXGraphix> m_SingleImages = new();

        private FXGraphix GetSingleImage(UIFxType type)
        {
            
            if(!m_SingleImages.TryGetValue(type, out var finImg))
            {
                finImg = GetPooledFxImage(type);
                m_SingleImages[type] = finImg;
            }
            
            return finImg;
        }

        public void DisableSingleImage( UIFxType type)
        {
            if (m_SingleImages.TryGetValue(type, out var finImg))
            {
                finImg.gameObject.SetActive(false);
                ReturnPooledFxImage(finImg);
                m_SingleImages.Remove(type);
            }
        }

        private void FinAtLocalCanvas(int orderInLayer,UIFxType type,Vector2 localCanvasPos, float dur, Color color, Vector2? size)
        {
            var uiFxImg = GetSingleImage(type);
            var rt = uiFxImg.CachedRectTransform;

            uiFxImg.gameObject.SetActive(false);
            rt.SetParent(m_UICanvas.transform, false);
            rt.anchoredPosition = localCanvasPos;

            if (size.HasValue)
            {
                rt.sizeDelta = size.Value;
            }

            uiFxImg.Fin(orderInLayer,dur, color);
        }

       

        
    }
    
    
   
}