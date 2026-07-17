#nullable enable
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
#if CADI_DOTWEEN
using DG.Tweening;
#endif

namespace Cadi.Scripts.UI.Extensions
{
    public static class UIExtensions
    {
        public static bool AdjustFor(
            this Canvas canvas,
            RenderMode renderMode,
            int sortingOrder,
            Camera extCam,
            Object? logContext = null)
        {
            if (canvas == null)
            {
                Debug.LogError("CanvasExtensions: Canvas is null.", logContext);
                return false;
            }

            canvas.renderMode = renderMode;

            if (renderMode == RenderMode.ScreenSpaceCamera)
            {
                if (canvas.worldCamera == null)
                {
                    Debug.LogWarning(
                        "CanvasExtensions: RenderMode ScreenSpaceCamera but no camera assigned. Trying to resolve UI camera...",
                        logContext);

                    var cam = extCam != null ? extCam : (canvas.worldCamera != null ? canvas.worldCamera : null);

                    if (cam != null)
                    {
                        canvas.worldCamera = cam;
                        //Debug.Log($"CanvasExtensions: Assigned camera '{cam.name}' to Canvas.", logContext);
                    }
                    else
                    {
                        Debug.LogError(
                            "CanvasExtensions: No UI camera resolved. Assign a camera or provide a resolver.",
                            logContext);

                        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                        Debug.LogWarning(
                            "CanvasExtensions: Falling back to ScreenSpaceOverlay.",
                            logContext);
                        return false;
                    }
                }
            }

            canvas.sortingOrder = sortingOrder;
            return true;
        }

        public static void SetAlpha(this Graphic graphic, float alpha)
        {
            if (!graphic) return;

            Color c = graphic.color;
            float a = Mathf.Clamp01(alpha);
            if (Mathf.Approximately(c.a, a)) return;

            c.a = a;
            graphic.color = c;
        }

        public static void SetSprite(this Image img, Sprite? sprite, bool setNativeSize = false,
            bool preserveAlpha = true, bool disableWhenNull = false)
        {
            if (!img) return;

            float a = preserveAlpha ? img.color.a : 1f;

            if (img.sprite != sprite)
                img.sprite = sprite;

            if (setNativeSize && sprite != null)
                img.SetNativeSize();

            if (disableWhenNull)
                img.enabled = sprite != null;

            if (preserveAlpha)
                img.SetAlpha(a);
        }


        public static void SetRaycastTarget(this Graphic graphic, bool enabled)
        {
            if (!graphic) return;
            if (graphic.raycastTarget == enabled) return;
            graphic.raycastTarget = enabled;
        }

        public static void AddAlpha(this Graphic graphic, float delta)
        {
            if (!graphic) return;

            Color c = graphic.color;
            float a = Mathf.Clamp01(c.a + delta);
            if (Mathf.Approximately(c.a, a)) return;

            c.a = a;
            graphic.color = c;
        }

        public static void DoFadeToggleImage(this Image img, float enable, float duration = 0.25f)
        {
            if (img == null)
                return;

            if (enable > 0f)
            {
                img.SetAlpha(0f);
                img.gameObject.SetActive(true);
            }
#if CADI_DOTWEEN

            img.DOFade(enable, duration).OnComplete(() =>
            {
                if (enable <= 0f)
                    img.gameObject.SetActive(false);
            });
#endif
        }


        public static void SetValue(this Slider slider, float value, bool notify = false)
        {
            if (!slider) return;

            float v = Mathf.Clamp(value, slider.minValue, slider.maxValue);
            if (Mathf.Approximately(slider.value, v))
            {
                if (!notify) return;
            }

            if (notify) slider.value = v;
            else slider.SetValueWithoutNotify(v);
        }


        public static void SetOnClick(this Button btn, Action action, bool clearExisting = true)
        {
            if (!btn) return;

            if (clearExisting) btn.onClick.RemoveAllListeners();
            if (action == null) return;

            btn.onClick.AddListener(() => action());
        }


        // -------------------------
        // RectTransform - layout utilities
        // -------------------------

        public static void SetAnchoredPosX(this RectTransform rt, float x)
        {
            if (!rt) return;
            Vector2 p = rt.anchoredPosition;
            if (Mathf.Approximately(p.x, x)) return;
            p.x = x;
            rt.anchoredPosition = p;
        }

        public static void SetAnchoredPosY(this RectTransform rt, float y)
        {
            if (!rt) return;
            Vector2 p = rt.anchoredPosition;
            if (Mathf.Approximately(p.y, y)) return;
            p.y = y;
            rt.anchoredPosition = p;
        }

        public static void SetAnchoredPosition(this RectTransform rt, Vector2 pos)
        {
            if (!rt) return;
            if (rt.anchoredPosition == pos) return;
            rt.anchoredPosition = pos;
        }

        public static void SetSize(this RectTransform rt, Vector2 size)
        {
            if (!rt) return;
            if (rt.sizeDelta == size) return;
            rt.sizeDelta = size;
        }

        public static void SetWidth(this RectTransform rt, float width)
        {
            if (!rt) return;
            Vector2 s = rt.sizeDelta;
            if (Mathf.Approximately(s.x, width)) return;
            s.x = width;
            rt.sizeDelta = s;
        }

        public static void SetHeight(this RectTransform rt, float height)
        {
            if (!rt) return;
            Vector2 s = rt.sizeDelta;
            if (Mathf.Approximately(s.y, height)) return;
            s.y = height;
            rt.sizeDelta = s;
        }

        public static void SetPivot(this RectTransform rt, Vector2 pivot)
        {
            if (!rt) return;
            if (rt.pivot == pivot) return;
            rt.pivot = pivot;
        }

        public static void SetAnchors(this RectTransform rt, Vector2 min, Vector2 max)
        {
            if (!rt) return;

            if (rt.anchorMin != min) rt.anchorMin = min;
            if (rt.anchorMax != max) rt.anchorMax = max;
        }

        /// <summary>Full stretch inside parent (anchors 0..1, offsets 0).</summary>
        public static void StretchToParent(this RectTransform rt)
        {
            if (!rt) return;

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.anchoredPosition = Vector2.zero;
            rt.sizeDelta = Vector2.zero;
        }

        /// <summary>Set offsets for stretch-anchored RectTransforms (Left/Right/Top/Bottom).</summary>
        public static void SetOffsets(this RectTransform rt, float left, float right, float top, float bottom)
        {
            if (!rt) return;

            // When stretched, offsetMin = (left, bottom), offsetMax = (-right, -top)
            Vector2 min = rt.offsetMin;
            Vector2 max = rt.offsetMax;

            bool changed = false;

            if (!Mathf.Approximately(min.x, left))
            {
                min.x = left;
                changed = true;
            }

            if (!Mathf.Approximately(min.y, bottom))
            {
                min.y = bottom;
                changed = true;
            }

            if (!Mathf.Approximately(max.x, -right))
            {
                max.x = -right;
                changed = true;
            }

            if (!Mathf.Approximately(max.y, -top))
            {
                max.y = -top;
                changed = true;
            }

            if (!changed) return;

            rt.offsetMin = min;
            rt.offsetMax = max;
        }

        public static Rect GetWorldRect(this RectTransform rt, Camera? cam = null)
        {
            if (!rt) return default;

            var corners = new Vector3[4];
            rt.GetWorldCorners(corners);

            Vector3 bl = corners[0];
            Vector3 tr = corners[2];

            if (cam != null)
            {
                bl = cam.WorldToScreenPoint(bl);
                tr = cam.WorldToScreenPoint(tr);
            }

            return Rect.MinMaxRect(bl.x, bl.y, tr.x, tr.y);
        }

        public static void SetFullStretch(this RectTransform rt, float margin)
        {
            if (rt == null)
            {
                return;
            }

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            rt.offsetMin = new Vector2(margin, margin);
            rt.offsetMax = new Vector2(-margin, -margin);

            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }

        public static void SetFullStretch(this RectTransform rt, float left, float right, float top, float bottom)
        {
            if (rt == null)
            {
                return;
            }

            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;

            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);

            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.localScale = Vector3.one;
            rt.localRotation = Quaternion.identity;
        }

        
#if CADI_DOTWEEN
        public static Sequence RevealImagesThen(Action onComplete, float revealDuration, params Image[] images)
        {
            Sequence seq = DOTween.Sequence();

            if (images == null || images.Length == 0)
            {
                seq.AppendCallback(() => onComplete?.Invoke());
                return seq;
            }

            for (int i = 0; i < images.Length; i++)
            {
                Image img = images[i];
                if (img == null)
                {
                    continue;
                }

                img.gameObject.SetActive(true);
                img.fillAmount = 0f;

                // Reveal from right to left requires the Image settings described above.
                seq.Join(img.DOFillAmount(1f, revealDuration).SetEase(Ease.InOutSine));
            }

            seq.AppendCallback(() => onComplete?.Invoke());
            return seq;
        }

        public static Tween? TypeText(TMP_Text tmp, string fullText, float charInterval, bool richText = true)
        {
            if (tmp == null)
            {
                return null;
            }

            tmp.richText = richText;
            tmp.text = "";

            int len = string.IsNullOrEmpty(fullText) ? 0 : fullText.Length;
            int idx = 0;

            // Use a tween purely as a timer to avoid allocations/coroutines.
            return DOTween.To(() => idx, v => idx = v, len, len * charInterval).SetEase(Ease.Linear)
                .OnUpdate(() =>
                {
                    int safe = Mathf.Clamp(idx, 0, len);
                    tmp.text = safe == 0 ? "" : fullText.Substring(0, safe);
                })
                .OnComplete(() => tmp.text = fullText);
        }

        public static Tween? CountUpInt(TMP_Text tmp, int from, int to, float duration, Func<int, string>? format)
        {
            if (tmp == null)
            {
                return null;
            }

            int value = from;
            tmp.text = format != null ? format(value) : value.ToString();

            return DOTween.To(() => value, v => value = v, to, duration)
                .SetEase(Ease.OutCubic)
                .OnUpdate(() => { tmp.text = format != null ? format(value) : value.ToString(); })
                .OnComplete(() => { tmp.text = format != null ? format(to) : to.ToString(); });
        }
        
#endif
    }
}