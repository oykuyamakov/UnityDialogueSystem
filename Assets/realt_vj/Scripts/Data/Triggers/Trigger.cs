using System;

namespace RealtVJ.Data
{
    [Serializable]
    public abstract class Trigger
    {
        public abstract TriggerType Type { get; }

        public abstract Trigger Clone();
    }
}
