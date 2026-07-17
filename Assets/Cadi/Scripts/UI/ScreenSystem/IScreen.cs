using System;
using Cadi.Scripts.EventSystem;
using UnityEngine;

namespace Cadi.Scripts.UI.ScreenSystem
{
    public interface IScreen
    {
        public void Sub();
        public void UnSub();
        
        public void OnShow();
        public void OnHide();
    }

    [Serializable]
    public class ScreenEventData
    {
        [SerializeField]
        public bool Show;
        
        public ScreenEventData(bool show)
        {
            Show = show;
        }
        public ScreenEventData() => Show = true;
    }

    public class ScreenEvent : Event<ScreenEvent>
    {
        public ScreenEventData ScreenData;

        public static ScreenEvent Rent(ScreenEventData data)
        {
            var evt = RentPooledInternal();
            evt.ScreenData = data;
            return evt;
        }

        protected override void Reset()
        {
            base.Reset();
            ScreenData = null;
        }
    }
}