using System.Collections.Generic;
using System.Linq;
using Cadi.Scripts.CacherSystem;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
using Sirenix.Utilities;
#endif
using UnityEngine;

namespace Cadi.Scripts.UI
{
    [RequireComponent(typeof(CanvasGroup))]
    [RequireComponent(typeof(Canvas))]
    [DisallowMultipleComponent]
    public class CanvasOrderPolice : CacherMonoBehaviour
    {
        [CachedField, SerializeField]
        protected Canvas m_SelfCanvas;

        [SerializeField]
        [CachedField(RefSearch.Children, includeInactive: true, required: false)]
        protected List<Canvas> m_CanvasRefs = new();
#if ODIN_INSPECTOR
        [ListDrawerSettings(
            DraggableItems = true,
            HideAddButton = true,
            HideRemoveButton = true,
            ListElementLabelName = nameof(CanvasOrderItem.Label)
        )][OnValueChanged(nameof(SortListByCurrentSortingOrder))][SerializeField]
#endif
        protected List<CanvasOrderItem> m_Canvases = new();

        public IReadOnlyList<Canvas> CanvasRefs => m_CanvasRefs;

        public int GetHighestOrder()
        {
            return m_Canvases.Last().SortingOrder;
        }

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            SortListByCurrentSortingOrder();
        }

        public void SortListByCurrentSortingOrder()
        {
            var orderedCanvases = m_CanvasRefs
                .Where(c => c != null)
                .OrderBy(c => c.sortingOrder)
                .ThenBy(c => c.transform.GetSiblingIndex())
                .ToList();

            if (m_Canvases == null || m_Canvases.Count != orderedCanvases.Count)
            {
                m_Canvases = new List<CanvasOrderItem>();
                foreach (var canvas in orderedCanvases)
                {
                    var newOrdered = new CanvasOrderItem(canvas);
                    m_Canvases.Add(newOrdered);
                }
            }
            else
            {
                m_Canvases.RemoveAll(c => c == null || c.Label == "Missing Canvas" || !m_CanvasRefs.Contains(c.Canvas));

                for (int i = 1; i < m_Canvases.Count; i++)
                {
                    if (m_Canvases[i].SortingOrder < m_Canvases[i - 1].SortingOrder)
                        m_Canvases[i].SetSortingOrder(m_Canvases[i - 1].SortingOrder + 1);
                }
            }
        }
    }

    [System.Serializable]
    public class CanvasOrderItem
    {
        public CanvasOrderItem(Canvas canvas)
        {
            m_Canvas = canvas;
        }
#if ODIN_INSPECTOR
        [SerializeField][InlineProperty][ReadOnly]
#endif
        private Canvas m_Canvas;

        public Canvas Canvas => m_Canvas;
#if ODIN_INSPECTOR
        [ShowInInspector, ReadOnly, InlineProperty]
#endif

        public int SortingOrder => m_Canvas != null ? m_Canvas.sortingOrder : 0;

        public void SetSortingOrder(int order)
        {
            if (m_Canvas != null)
                m_Canvas.sortingOrder = order;
        }

        public string Label => m_Canvas != null
            ? $"{SortingOrder} - {m_Canvas.name}"
            : "Missing Canvas";
    }
}