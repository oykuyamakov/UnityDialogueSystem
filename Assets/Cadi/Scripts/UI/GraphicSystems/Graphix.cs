using Cadi.Scripts.CacherSystem;
using Cadi.Scripts.UI.Extensions;
using Cadi.Scripts.UI.FX;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.UI;

namespace Cadi.Scripts.UI.GraphicSystems
{
    public class Graphix : CacherMonoBehaviour
    {
#if ODIN_INSPECTOR
        [FoldoutGroup("Settings"), ShowIf(nameof(ShowSlot))]
        [InlineProperty, LabelText("Root Slot")]
#endif
        [SerializeField]
        protected Slot m_Slot = new();

        [CachedField, SerializeField, HideInInspector]
        protected RectTransform m_RectTransform;

        public Slot ThisSlot => m_Slot;
        public bool ContentSet { get; protected set; }
        protected virtual bool ShowSettings => true;
        protected virtual bool ShowSlot => ShowSettings;
        protected virtual float ContentPadding => 0;

        public virtual IsNested IsNested => IsNested.Single;
        public RectTransform CachedRectTransform => m_RectTransform;

        public virtual void SetContent(Sprite content, bool preserveAspect, bool fullStretch)
        {
            SetContentInternal(m_Slot, content, preserveAspect, fullStretch);
        }

        public virtual void SetContent(Texture content)
        {
            RuntimeBind();
            
            m_Slot.SetTexture(content);
            ContentSet = true;
        }

        protected void SetContentInternal(
            Slot slot,
            Sprite content,
            bool preserveAspect,
            bool fullStretch)
        {
            slot.SetSprite(content);
            ContentSet = true;

            if (slot.Graphic is Image img)
            {
                var rc = img.rectTransform;

                if (preserveAspect)
                {
                    img.preserveAspect = true;

                    if (fullStretch)
                    {
                        rc.SetFullStretch(ContentPadding);
                    }
                    else
                    {
                        img.SetNativeSize();
                        rc.anchorMin = new Vector2(0.5f, 0.5f);
                        rc.anchorMax = new Vector2(0.5f, 0.5f);
                        rc.anchoredPosition = Vector2.zero;
                    }
                }
                else
                {
                    img.preserveAspect = false;
                    rc.SetFullStretch(ContentPadding);
                }
            }
        }
        
        protected virtual void RuntimeBind()
        {
            if (m_Slot.Graphic != null)
                return;

            m_Slot.Bind(
                GetComponent<Graphic>(),
                GetComponent<UIOutline>()
            );
        }

        public virtual void UpdateVisuals(bool selected)
        {
            ApplySlotVisual(m_Slot, selected);
        }

        protected static void ApplySlotVisual(Slot slot, bool selected)
        {
            if (selected)
                slot.ApplySelected();
            else
                slot.ApplyDefault();
        }

        protected virtual void OnDestroy()
        {
            m_Slot.Dispose();
        }

        // -----------------------------------------------------------
        // Editor
        // -----------------------------------------------------------

#if UNITY_EDITOR
        protected virtual void EditorSync()
        {
            EditorSyncSlot(m_Slot, gameObject);
        }

        protected void EditorSyncSlot(Slot slot, GameObject host)
        {
            if (host == null || !host.activeInHierarchy)
                return;

            EnsureGraphic(host, slot.Type);

            if (CorrectType(host, slot))
                return;

            BindSlot(slot, host);
            SyncOutline(slot, host);
            slot.ApplyDefault();
        }

        protected override void OnValidated()
        {
            base.OnValidated();
            EditorSync();
        }

        protected override void OnReset()
        {
            EditorSync();
        }
#if ODIN_INSPECTOR
        [Button("Refresh"), PropertyOrder(100)]
        [FoldoutGroup("Settings")]
        [ShowIf(nameof(ShowSettings))]
#endif
        private void EditorRefresh()
        {
            EditorSync();
            UnityEditor.EditorUtility.SetDirty(this);
        }

        private static void EnsureGraphic(GameObject host, GraphicType type)
        {
            if (host.GetComponent<Graphic>() != null)
                return;

            switch (type)
            {
                case GraphicType.Image:
                    host.AddComponent<Image>();
                    break;
                case GraphicType.Raw:
                    host.AddComponent<RawImage>();
                    break;
            }
        }

        private static bool CorrectType(GameObject host, Slot slot)
        {
            var current = host.GetComponent<Graphic>();
            if (current == null)
                return false;

            GraphicType type = slot.Type;

            bool alreadyCorrect =
                type == GraphicType.Image && current is Image ||
                type == GraphicType.Raw && current is RawImage;

            if (alreadyCorrect)
                return false;

            var hostRef = host;
            var slotRef = slot;
            var typeRef = type;

            UnityEditor.EditorApplication.delayCall += () =>
            {
                if (hostRef == null)
                    return;

                var oldGraphic = hostRef.GetComponent<Graphic>();

                if (oldGraphic != null)
                    DestroyImmediate(oldGraphic);

                Graphic newGraphic = null;

                switch (typeRef)
                {
                    case GraphicType.Image:
                        newGraphic = hostRef.AddComponent<Image>();
                        break;

                    case GraphicType.Raw:
                        newGraphic = hostRef.AddComponent<RawImage>();
                        break;
                }

                var outline = hostRef.GetComponent<UIOutline>();

                slotRef.Bind(newGraphic, outline);

                if (slotRef.UseOutline)
                {
                    if (outline == null)
                        outline = hostRef.AddComponent<UIOutline>();

                    slotRef.Bind(newGraphic, outline);
                    slotRef.ApplyOutlineSettings();
                }
                else if (outline != null)
                {
                    outline.enabled = false;
                }

                slotRef.ApplyDefault();

                UnityEditor.EditorUtility.SetDirty(hostRef);
            };

            return true;
        }

        private static void BindSlot(Slot slot, GameObject host)
        {
            slot.Bind(
                host.GetComponent<Graphic>(),
                host.GetComponent<UIOutline>()
            );
        }

        private static void SyncOutline(Slot slot, GameObject host)
        {
            if (slot.UseOutline)
            {
                var outline = host.GetComponent<UIOutline>();
                if (outline == null)
                    outline = host.AddComponent<UIOutline>();

                outline.enabled = true;
                slot.Bind(host.GetComponent<Graphic>(), outline);
                slot.ApplyOutlineSettings();
            }
            else
            {
                var outline = host.GetComponent<UIOutline>();
                if (outline != null)
                    outline.enabled = false;
            }
        }
#endif
    }
}