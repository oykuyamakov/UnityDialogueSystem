using Cadi.Scripts.EventSystem;
using UnityEngine;
using UnityEngine.EventSystems;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif

namespace Cadi.Scripts.UI.GraphicSystems.Selective
{
    public class Selectix : Graphix, IPointerDownHandler, ISelectix
    {
        [SerializeField]
#if ODIN_INSPECTOR
         [FoldoutGroup("Settings/Selection"), InlineProperty, LabelText("Selection")][ShowIf(nameof(ShowSettings))]
#endif
        private SelectionController m_Selection = new();

        protected override bool ShowSettings => m_ShowSettings;

        public int RuntimeID => m_Selection.RuntimeID;
        public bool IsLocked => m_Selection.IsLocked;
        
        private bool m_ShowSettings;

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


        public void Init() =>
            m_Selection.BindToGraphix(this);
      
        public void SetGroup(int runtimeId, SelectixGroup group)
        {
            m_Selection.SetGroup(runtimeId, group);
            
            m_Slot.ApplyDefault();
        }
        public void Lock(bool disableVis)
        {
            m_Selection.Lock(disableVis);
            
            if (disableVis)
            {
                m_Slot.ApplyDefault();
            }
        } 
        public void UnLock() => m_Selection.UnLock();

        public void TryDeselect() => m_Selection.TryDeselect();


        public void EditorBind(SelectixGroup group)
        {
            if (group == null)
                return;

            m_ShowSettings = group.AllowOverridenChildren;
            
            m_Slot.CopyFrom(group.RootSettings);
            m_Selection.EditorBind(group);

#if UNITY_EDITOR
            if (!Application.isPlaying)
                EditorSync();
            if (!Application.isPlaying)
                UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }
}
