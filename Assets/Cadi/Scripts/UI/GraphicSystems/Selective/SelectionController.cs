using System;
using Cadi.Scripts.EventSystem;
using Cadi.Scripts.UI.FX;
using UnityEngine;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Cadi.Scripts.UI.GraphicSystems.Selective
{
    [Serializable]
#if ODIN_INSPECTOR
    [InlineProperty]
#endif
    public class SelectionController
    {
        [SerializeField, HideInInspector]
        private int m_RuntimeID = -1;

        [SerializeField]
        private bool m_MultiSelectable;
        [SerializeField]
        private bool m_Deselectable;
        [SerializeField]
        private UIFxType m_FxForeground;
        [SerializeField]
        private UIFxType m_FxBackground;
        
        [SerializeField] 
        private SelectixGroup m_Group;
        
        // -----------------------------------------------------------
        // Runtime state (non-serialized)
        // -----------------------------------------------------------

        [NonSerialized] 
        private bool m_Selected;
[NonSerialized]
        private bool m_Locked;

        [NonSerialized]
        private RectTransform m_OwnerRT;
        [NonSerialized]
        private UIFXPooler m_Pooler;
        
        [NonSerialized]
        private FXGraphix m_ActiveFxBg;
        [NonSerialized]
        private FXGraphix m_ActiveFxFg;
        
        private ISelectix m_Owner;
        
        [NonSerialized]
        private bool m_Subscribed;
        
        private bool m_ExposeSettings = false;

        // -----------------------------------------------------------
        // Properties
        // -----------------------------------------------------------

        public int RuntimeID => m_RuntimeID;
        public bool IsSelected => m_Selected;
        public bool IsLocked => m_Locked;
        public bool GroupSet => m_Group != null;
        public SelectixGroup Group => m_Group;

        public bool MultiSelectable
        {
            get => m_MultiSelectable;
            set => m_MultiSelectable = value;
        }

        public bool Deselectable
        {
            get => m_Deselectable;
            set => m_Deselectable = value;
        }

        public UIFxType FxForeground
        {
            get => m_FxForeground;
            set => m_FxForeground = value;
        }

        public UIFxType FxBackground
        {
            get => m_FxBackground;
            set => m_FxBackground = value;
        }

        public void Subscribe()
        {
            if (m_Group == null || m_Subscribed)
                return;

            EM.AddListener<SGraphixSelectedEvent>(OnOtherSelected, Priority.High, m_Group);
            m_Subscribed = true;
        }

        public void Unsubscribe()
        {
            if (m_Group == null || !m_Subscribed)
                return;

            EM.RemoveListener<SGraphixSelectedEvent>(OnOtherSelected, m_Group);
            m_Subscribed = false;
        }
        
        public void OnDisable()
        {
            ResetLockRespectful();
        }

        public void OnDestroy()
        {
            Unsubscribe();
            DisableSelectionFx();
        }

        // -----------------------------------------------------------
        // Core selection
        // -----------------------------------------------------------

        public bool TrySelect()
        {
            if (m_Selected || m_Locked)
                return false;

            m_Selected = true;
            m_Owner.UpdateVisuals(true);

            SpawnSelectionFx();
            
            using var evt = SGraphixSelectedEvent.Rent(m_Owner);
            EM.SendEvent(evt, m_Group);
            return true;
        }

        public void SlotVis(Slot slot, bool visibility)
        {
            if (visibility)
            {
                slot.ApplySelected();
            }
            else
            {
                slot.ApplyDefault();
            }
        }

        public bool TryDeselect()
        {
            if (!m_Selected || m_Locked)
                return false;

            m_Selected = false;
            DisableSelectionFx();

            using var evt = SGraphixDeselectedEvent.Rent(m_Owner);
            EM.SendEvent(evt, m_Group);

            m_Owner.UpdateVisuals(false);
            return true;
        }

        public bool HandlePointerDown()
        {
            if (m_Locked)
                return false;

            if (m_Selected)
            {
                if (m_Deselectable)
                    return TryDeselect();

                return false;
            }

            return TrySelect();
        }
        
        public void Lock(bool disaleFx)
        {
            m_Locked = true;
            if(disaleFx)
                DisableSelectionFx();
        }

        public void UnLock() => m_Locked = false;
        public void ResetLockRespectful()
        {
            m_Selected = false;
            
            DisableSelectionFx();
            
            if(m_Owner == null)
                return;

            using var evt = SGraphixDeselectedEvent.Rent(m_Owner);
            EM.SendEvent(evt, m_Group);
            
            m_Owner.UpdateVisuals(false);
        }

        // -----------------------------------------------------------
        // Group integration
        // -----------------------------------------------------------

        public void SetGroup(int runtimeId, SelectixGroup group)
        {
            if (group == null)
                return;
            
            if (GroupSet && m_Group != group)
            {
                Debug.LogWarning($"selectix with 2 groups not allowed");
                return;
            }
        
            m_RuntimeID = runtimeId;
            m_Group = group;

            m_Locked = false;
            DisableSelectionFx();
            
            Subscribe();
        }

        public void BindToGraphix(ISelectix owner)
        {
            m_Owner = owner;
            m_Pooler = UIFXPooler.Instance;
            m_OwnerRT = m_Owner.CachedRectTransform;
        }

        public void EditorBind(SelectixGroup group)
        {
            m_Group = group;

            if (group == null)
                return;

            m_MultiSelectable = group.AllowMultipleSelection;
            m_Deselectable = group.AllowDeselection;
            m_FxForeground = group.FxForeground;
            m_FxBackground = group.FxBackground;
        }

        // -----------------------------------------------------------
        // Private
        // -----------------------------------------------------------

        private void OnOtherSelected(SGraphixSelectedEvent evt)
        {
            if (evt.Selectix != null && evt.Selectix == m_Owner)
                return;

            if (m_Locked)
                return;

            if (m_MultiSelectable)
                return;

            if (m_Selected)
                TryDeselect();
        }

        private void SpawnSelectionFx()
        {
            if (m_Pooler == null || m_Group == null || m_OwnerRT == null)
                return;

            DisableSelectionFx();

            var rt = m_OwnerRT;
            int sortOrder = m_Group.SortingOrder;

            float sizer = rt.sizeDelta.sqrMagnitude < 210000 ? 0.5f : 1f;
            
            if (m_FxBackground != UIFxType.None)
                m_ActiveFxBg = m_Pooler.ShowFxAtRect(m_FxBackground, rt, Color.white, 0.5f, sortOrder,sizer );

            if (m_FxForeground != UIFxType.None)
                m_ActiveFxFg = m_Pooler.ShowFxAtRect(m_FxForeground, rt, Color.white, 0.5f, sortOrder + 1,1);
        }

        private void DisableSelectionFx()
        {
            if (m_Pooler == null)
                return;

            if (m_ActiveFxBg != null)
            {
                m_Pooler.HideFxImage(m_ActiveFxBg);
                m_ActiveFxBg = null;
            }

            if (m_ActiveFxFg != null)
            {
                m_Pooler.HideFxImage(m_ActiveFxFg);
                m_ActiveFxFg = null;
            }
        }
    }
}
