using System.Collections.Generic;
using System.Linq;
using Cadi.Scripts.CacherSystem;
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#else
using Cadi.Scripts.CustomAttributes;
#endif
using UnityEngine;

namespace Cadi.Scripts.UI
{
    public class CanvasPD : CacherMonoBehaviour
    {
        [SerializeField]
#if ODIN_INSPECTOR
        [ReadOnly,ListDrawerSettings(DraggableItems = false, HideAddButton = true, HideRemoveButton = true,
            ListElementLabelName = nameof(CanvasOrderPolice.GetHighestOrder), DefaultExpandedState = true)]
#endif
        private List<CanvasOrderPolice> m_Polices;

        public override void ResolveReferences()
        {
            base.ResolveReferences();
            Refresh();
        }

        [Button]
        public void Refresh()
        {
            var polices = GameObject.FindObjectsByType<CanvasOrderPolice>(sortMode: FindObjectsSortMode.None,  findObjectsInactive: FindObjectsInactive.Include);
            m_Polices = polices.OrderBy(c => c.GetHighestOrder()).ToList();
        }
    }
}