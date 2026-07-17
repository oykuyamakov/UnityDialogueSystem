using Cadi.Scripts.EventSystem;
using UnityEngine;
using UnityEngine.EventSystems;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Cadi.Scripts.UI.GraphicSystems.Selective
{
    public class NestedSelectix : NestedGraphix, IPointerDownHandler, ISelectix
    {
        [SerializeField]
#if ODIN_INSPECTOR
        [FoldoutGroup("Settings/Selection"), ShowIf(nameof(ShowSettings)),InlineProperty, LabelText("Selection")]
#endif
        private SelectionController m_Selection = new();

        [SerializeField, HideInInspector]
        private bool m_ShowSettings = false;

        protected override bool ShowSettings => m_ShowSettings;
        public int RuntimeID => m_Selection.RuntimeID;
        public bool IsLocked => m_Selection.IsLocked;

        private void Start()
        {
            Init();
            
            using var evt = SGraphixCreatedEvent.Rent(this);
            EM.SendEvent(evt, m_Selection.Group);
        }

        private void OnDisable()
        {
            m_Selection.OnDisable();
            UpdateVisuals(false);
        }

        protected override void OnDestroy()
        {
            m_Selection.OnDestroy();
            base.OnDestroy();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            m_Selection.HandlePointerDown();
        }

        // -----------------------------------------------------------
        // Public API
        // -----------------------------------------------------------

        public void Init()
            => m_Selection.BindToGraphix(this);
        
        public void SetGroup(int runtimeId, SelectixGroup group)
        {
            m_Selection.SetGroup(runtimeId, group);
            UpdateVisuals(false);
        }
        public void Lock(bool disableFX)
        {
            m_Selection.Lock(disableFX);
            UpdateVisuals(false);
        }

        public void UnLock() => m_Selection.UnLock();

        public void TryDeselect() => m_Selection.TryDeselect();
        
        public void EditorBind(SelectixGroup group)
        {
            if (group == null)
            {
                m_ShowSettings = true;
                return;
            }

            var prev = m_ShowSettings;

            m_ShowSettings = group.AllowOverridenChildren;

            if (!m_ShowSettings || prev != m_ShowSettings)
            {
                m_Selection.EditorBind(group);
                m_TargetContentPadding = group.ContentPadding;

                m_Mode = group.IsNested;
                // Push color settings to slots
                m_Slot.CopyFrom(group.RootSettings);

                m_ChildSlot.CopyFrom(group.ChildSlot);
                
#if UNITY_EDITOR
                // These require editor-time asset changes
                // Mode and type are set, then EditorSync refreshes hierarchy
                EditorSync();
                UnityEditor.EditorUtility.SetDirty(this);
#endif
            }
        }

        protected bool EnableOverrideEditorCheck()
        {
#if UNITY_EDITOR
            if (m_Selection.Group != null)
            {
                EditorBind(m_Selection.Group);
                return m_Selection.Group.AllowOverridenChildren;
            }

            var parent = transform.parent;

            if (parent != null && parent.TryGetComponent(out SelectixGroup group))
            {
                EditorBind(group);
                return group.AllowOverridenChildren;
            }

            return true;
#else
            return false;
#endif
        }
    }
}
