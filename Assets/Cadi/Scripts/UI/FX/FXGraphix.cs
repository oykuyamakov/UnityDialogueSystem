using System;
using System.Collections;
using System.Collections.Generic;
using Cadi.Scripts.CacherSystem;
using Cadi.Scripts.UI.GraphicSystems;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace Cadi.Scripts.UI.FX
{
    [RequireComponent(typeof(Image))]
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Canvas))]
    public class FXGraphix : Graphix
    {
        [SerializeField]
        private UIFxType m_FxType;

        [CachedField(addComponentIfMissing: true), SerializeField]
        protected Canvas m_Canvas;

        public UIFxType FxType => m_FxType;

        protected override bool ShowSlot => false;
        protected override bool ShowSettings => true;
        
        private Coroutine m_Routine;

#if UNITY_EDITOR
        protected override void EditorSync()
        {
            m_Slot.Type = GraphicType.Image;
            base.EditorSync();
        }
#endif

        // -----------------------------------------------------------
        // Public API
        // -----------------------------------------------------------

        public void Initialize()
        {
            gameObject.SetActive(false);
        }

        public void SetOrder(int order)
        {
            m_Canvas.overrideSorting = true;
            order = m_FxType == UIFxType.DefaultBack ? order - 1 : order + 1;
            m_Canvas.sortingOrder = order;
        }

        public void Sizer(float gecici)
        {
            transform.localScale = Vector3.one * gecici;
        }

        public void FinFout(int orderInLayer, float duration, Color color, Action<FXGraphix> onComplete)
        {
            StopFxRoutine();
            
            Sizer(1);
            m_Slot.SetColor(new Color(color.r, color.g, color.b, 0f));
            gameObject.SetActive(true);

            SetOrder(orderInLayer);
            m_Routine = StartCoroutine(CoFinFout(duration, color, onComplete));
        }

        private IEnumerator CoFinFout(float duration, Color color, Action<FXGraphix> onComplete)
        {
            float fadeIn = Mathf.Min(0.12f, duration * 0.25f);
            float fadeOut = fadeIn;
            float hold = Mathf.Max(0f, duration - (fadeIn + fadeOut));

            // Fade In
            float t = 0f;
            while (t < fadeIn)
            {
                t += Time.unscaledDeltaTime;
                float a = fadeIn > 0f ? Mathf.Clamp01(t / fadeIn) : 1f;
                m_Slot.SetColor(new Color(color.r, color.g, color.b, a));
                yield return null;
            }

            // Hold
            float h = 0f;
            while (h < hold)
            {
                h += Time.unscaledDeltaTime;
                yield return null;
            }

            // Fade Out
            t = 0f;
            while (t < fadeOut)
            {
                t += Time.unscaledDeltaTime;
                float a = fadeOut > 0f ? 1f - Mathf.Clamp01(t / fadeOut) : 0f;
                m_Slot.SetColor(new Color(color.r, color.g, color.b, a));
                yield return null;
            }

            onComplete.Invoke(this);
        }

        public void Fin(int orderInLayer, float duration, Color color, Action onComplete = null)
        {
            StopFxRoutine();
            
            Sizer(1);
            m_Slot.SetColor(new Color(color.r, color.g, color.b, 0f));
            gameObject.SetActive(true);
            SetOrder(orderInLayer);
            m_Routine = StartCoroutine(CoFin(duration, color, onComplete));
        }

        private IEnumerator CoFin(float duration, Color color, Action onComplete)
        {
            float fadeIn = Mathf.Min(0.12f, duration * 0.25f);

            float t = 0f;
            while (t < fadeIn)
            {
                t += Time.unscaledDeltaTime;
                float a = fadeIn > 0f ? Mathf.Clamp01(t / fadeIn) : 1f;
                m_Slot.SetColor(new Color(color.r, color.g, color.b, a));
                yield return null;
            }

            onComplete?.Invoke();
        }
        
        private void StopFxRoutine()
        {
            if (m_Routine == null)
                return;

            StopCoroutine(m_Routine);
            m_Routine = null;
        }
        
        public void StopFx()
        {
            StopFxRoutine();
        }
    }

    public static class ImagePrefabHelpers
    {
        public static Stack<FXGraphix> InitializeStack(FXGraphix prefab, Transform canvas, int size)
        {
            if (!Application.isPlaying)
                return null;

            if (prefab == null)
            {
                Debug.LogError("UI FX prefab is null. Please assign a prefab in Content SO.");
                return null;
            }

            var stack = new Stack<FXGraphix>(size);

            for (int i = 0; i < size; i++)
            {
                var fx = Create(prefab, canvas);
                stack.Push(fx);
            }

            return stack;
        }

        public static FXGraphix Create(FXGraphix prefab, Transform canvas)
        {
            var fx = Object.Instantiate(prefab, canvas);
            fx.Initialize();
            return fx;
        }

        public static bool TryCreate(UIFxType type, Transform canvas, out FXGraphix fx)
        {
            fx = null;
            var prefab = UIContent.Get().Get(type);

            if (prefab == null)
                return false;

            fx = Object.Instantiate(prefab, canvas);
            fx.Initialize();
            return true;
        }
    }

    [Serializable]
    public enum UIFxType
    {
        None,
        DefaultBack,
        DefaultFront
    }
}