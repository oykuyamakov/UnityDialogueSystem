using Cadi.Scripts.UI.Extensions;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Cadi.Scripts.UI.GraphicSystems
{
    public class NestedGraphix : Graphix
    {
#if ODIN_INSPECTOR
        [FoldoutGroup("Settings"), ShowIf(nameof(ShowSettings))]
#endif
        [SerializeField]
        protected IsNested m_Mode;

#if ODIN_INSPECTOR
        [FoldoutGroup("Settings"), ShowIf(nameof(ShowSettings))]
#endif
        [SerializeField]
        protected float m_TargetContentPadding = 25f;
#if ODIN_INSPECTOR
        [FoldoutGroup("Settings"), ShowIf(nameof(ShowChildSlot))]
        [InlineProperty, LabelText("Child Slot")]
#endif
        [SerializeField]
        protected Slot m_ChildSlot = new();

        [SerializeField, HideInInspector]
        private RectTransform m_MainChild;
        protected override float ContentPadding => m_TargetContentPadding;
        public Slot ChildSlot => m_ChildSlot;

        public override IsNested IsNested => m_Mode;

        public Slot GetSlot(SlotLoc location)
        {
            return location == SlotLoc.Child ? m_ChildSlot : m_Slot;
        }

        public void SetContent(
            Sprite content,
            bool preserveAspect,
            bool fullStretch,
            SlotLoc location)
        {
            SetContentInternal(GetSlot(location), content, preserveAspect, fullStretch);
        }

        public void SetContent(Texture content, SlotLoc location)
        {
            GetSlot(location).SetTexture(content);
            ContentSet = true;
        }
        public override void UpdateVisuals(bool selected)
        {
            ApplySlotVisual(m_Slot, selected);

            if (m_Mode == IsNested.Nested)
                ApplySlotVisual(m_ChildSlot, selected);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            m_ChildSlot.Dispose();
        }
     
        // -----------------------------------------------------------
        // Editor
        // -----------------------------------------------------------

#if UNITY_EDITOR
        protected override void EditorSync()
        {
            EditorSyncSlot(m_Slot, gameObject);

            if (m_Mode == IsNested.Nested)
            {
                EnsureMainChild();
                EditorSyncSlot(m_ChildSlot, m_MainChild.gameObject);
                m_MainChild.SetFullStretch(ContentPadding);
            }

            SyncEnabledStates();
        }

        private void EnsureMainChild()
        {
            if (m_MainChild != null)
                return;

            Transform existing = transform.Find("Main");

            if (existing != null)
            {
                m_MainChild = existing.GetComponent<RectTransform>();

                if (m_MainChild == null)
                {
                    Debug.LogWarning(
                        $"{nameof(NestedGraphix)} found a child named Main, but it does not have a RectTransform. Creating a UI Main child instead.",
                        this
                    );

                    existing.name = "Main_NonUI";
                }
            }

            if (m_MainChild == null)
            {
                var go = new GameObject("Main", typeof(RectTransform), typeof(Image));
                go.transform.SetParent(transform, false);
                m_MainChild = go.GetComponent<RectTransform>();
            }

            if (m_MainChild.GetComponent<Graphic>() == null)
            {
                m_MainChild.gameObject.AddComponent<Image>();
            }
        }

        private void SyncEnabledStates()
        {
            if (m_ChildSlot.Graphic != null)
                m_ChildSlot.Graphic.enabled = m_Mode == IsNested.Nested;

            if (m_Slot.Graphic != null)
                m_Slot.Graphic.enabled = true;
        }

        private void ApplyLayout()
        {
            if (m_MainChild == null)
                return;

            m_MainChild.SetFullStretch(ContentPadding);
        }
#endif
        
        
        protected virtual bool ShowTargetSetting => ShowSettings && m_Mode == IsNested.Nested;
        protected virtual bool ShowChildSlot => ShowSettings && m_Mode == IsNested.Nested;
    }
}
