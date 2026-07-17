using Cadi.Scripts.EventSystem;
using Cadi.Scripts.UI.GraphicSystems;
using Cadi.Scripts.UI.GraphicSystems.Selective;

namespace Cadi.Scripts.UI
{
    public class UIEvents
    {
    }

    public class SGraphixSelectedEvent : Event<SGraphixSelectedEvent>
    {
        public ISelectix Selectix { get; private set; }

        public static SGraphixSelectedEvent Rent(ISelectix selectix)
        {
            var e = Rent();
            e.Selectix = selectix;
            return e;
        }


        protected override void Reset()
        {
            base.Reset();
            Selectix = null;
        }
    }

    public class SGraphixDeselectedEvent : Event<SGraphixDeselectedEvent>
    {
        public ISelectix Selectix { get; private set; }

        public static SGraphixDeselectedEvent Rent(ISelectix selectix)
        {
            var e = Rent();
            e.Selectix = selectix;
            return e;
        }

        protected override void Reset()
        {
            base.Reset();
            Selectix = null;
        }
    }

    public class SGraphixCreatedEvent : Event<SGraphixCreatedEvent>
    {
        public ISelectix Selectix { get; private set; }

        public static SGraphixCreatedEvent Rent(ISelectix selectix)
        {
            var e = Rent();
            e.Selectix = selectix;
            return e;
        }


        protected override void Reset()
        {
            base.Reset();
            Selectix = null;
        }
    }
}