using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Cadi.Scripts.UI.Helpers
{
    [DisallowMultipleComponent]
    public sealed class UISlicedSpriteAnimator : MonoBehaviour
    {
        private const string c_BuiltChildPrefix = "[Slice] ";

        [Header("Sprites")]
        [SerializeField]
        private Sprite[] m_Sprites = Array.Empty<Sprite>();

        [Header("Playback")]
        [SerializeField, Min(0f)]
        private float m_Delay = 0.25f;

        [SerializeField]
        private bool m_Loop = true;

        [SerializeField]
        private bool m_PlayOnEnable = true;

        [SerializeField]
        private bool m_UseUnscaledTime;

        [Header("Build")]
        [SerializeField]
        private bool m_BuildOnAwake = true;

        [SerializeField]
        private bool m_ClearPreviousBuiltImages = true;

        [SerializeField]
        private Image[] m_Images = Array.Empty<Image>();

        private Coroutine m_Coroutine;

        private void Awake()
        {
            if (m_BuildOnAwake && !HasAnyValidImage())
                Build();
        }

        private void OnEnable()
        {
            if (m_PlayOnEnable)
                Play();
        }

        private void OnDisable()
        {
            Stop(hideImages: true);
        }

        [ContextMenu("Build")]
        public void Build()
        {
            if (m_ClearPreviousBuiltImages)
                ClearBuiltImages();

            if (m_Sprites == null || m_Sprites.Length == 0)
            {
                m_Images = Array.Empty<Image>();
                return;
            }

            m_Images = new Image[m_Sprites.Length];

            for (int i = 0; i < m_Sprites.Length; i++)
            {
                Sprite sprite = m_Sprites[i];

                if (sprite == null)
                    continue;

                Image image = CreateSliceImage(sprite);
                m_Images[i] = image;
            }

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }

        public void Play()
        {
            if (!isActiveAndEnabled)
                return;

            if (!HasAnyValidImage() && m_BuildOnAwake)
                Build();

            if (!HasAnyValidImage())
                return;

            Stop(hideImages: false);
            m_Coroutine = StartCoroutine(RevealRoutine());
        }

        public void Stop(bool hideImages = false)
        {
            if (m_Coroutine != null)
            {
                StopCoroutine(m_Coroutine);
                m_Coroutine = null;
            }

            if (hideImages)
                HideAll();
        }

        public void Restart()
        {
            Stop(hideImages: true);
            Play();
        }

        public void SetSprites(Sprite[] sprites, bool rebuild = true)
        {
            m_Sprites = sprites ?? Array.Empty<Sprite>();

            if (rebuild)
                Build();
        }

        private Image CreateSliceImage(Sprite sprite)
        {
            GameObject go = new GameObject(
                $"{c_BuiltChildPrefix}{sprite.name}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image)
            );

            go.transform.SetParent(transform, worldPositionStays: false);

            Image image = go.GetComponent<Image>();
            image.sprite = sprite;
            image.enabled = false;
            image.raycastTarget = false;
            image.SetNativeSize();

            RectTransform rectTransform = go.GetComponent<RectTransform>();
            ApplySpriteOriginalPosition(rectTransform, image, sprite);

            return image;
        }

        private static void ApplySpriteOriginalPosition(RectTransform rectTransform, Image image, Sprite sprite)
        {
            Rect spriteRect = sprite.rect;
            Texture texture = sprite.texture;

            float pixelsPerUnit = Mathf.Max(0.0001f, image.pixelsPerUnit);

            float x = spriteRect.x + spriteRect.width * 0.5f - texture.width * 0.5f;
            float y = spriteRect.y + spriteRect.height * 0.5f - texture.height * 0.5f;

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = new Vector2(x / pixelsPerUnit, y / pixelsPerUnit);
        }

        private IEnumerator RevealRoutine()
        {
            object delay = GetDelayYield();

            do
            {
                HideAll();

                for (int i = 0; i < m_Images.Length; i++)
                {
                    Image image = m_Images[i];

                    if (image == null)
                        continue;

                    yield return delay;
                    image.enabled = true;
                }

                yield return delay;
            } while (m_Loop);

            m_Coroutine = null;
        }

        private object GetDelayYield()
        {
            if (m_Delay <= 0f)
                return null;

            return m_UseUnscaledTime
                ? new WaitForSecondsRealtime(m_Delay)
                : new WaitForSeconds(m_Delay);
        }

        private void HideAll()
        {
            if (m_Images == null)
                return;

            for (int i = 0; i < m_Images.Length; i++)
            {
                if (m_Images[i] != null)
                    m_Images[i].enabled = false;
            }
        }

        private bool HasAnyValidImage()
        {
            if (m_Images == null || m_Images.Length == 0)
                return false;

            for (int i = 0; i < m_Images.Length; i++)
            {
                if (m_Images[i] != null)
                    return true;
            }

            return false;
        }

        private void ClearBuiltImages()
        {
            HashSet<GameObject> objectsToDestroy = new();

            if (m_Images != null)
            {
                for (int i = 0; i < m_Images.Length; i++)
                {
                    if (m_Images[i] != null)
                        objectsToDestroy.Add(m_Images[i].gameObject);
                }
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

                if (child.name.StartsWith(c_BuiltChildPrefix, StringComparison.Ordinal))
                    objectsToDestroy.Add(child.gameObject);
            }

            foreach (GameObject go in objectsToDestroy)
                DestroyObject(go);

            m_Images = Array.Empty<Image>();
        }

        private static void DestroyObject(GameObject go)
        {
            if (go == null)
                return;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                UnityEngine.Object.DestroyImmediate(go);
                return;
            }
#endif

            UnityEngine.Object.Destroy(go);
        }
    }
}