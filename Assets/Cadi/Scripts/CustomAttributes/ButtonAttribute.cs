using System;

namespace Cadi.Scripts.CustomAttributes
{
#if ODIN_INSPECTOR
    // When Odin is present, inherit from Odin's ButtonAttribute so Odin renders
    // [Button]-decorated methods natively through its own inspector pipeline.
    // User scripts require no changes — they keep using Cadi's namespace as usual.
    public class ButtonAttribute : Sirenix.OdinInspector.ButtonAttribute
    {
        public string Label { get; }

        public ButtonAttribute(string label = null) : base(label)
        {
            Label = label;
        }
    }
#else
    [AttributeUsage(AttributeTargets.Method, Inherited = true, AllowMultiple = false)]
    public class ButtonAttribute : Attribute
    {
        public readonly string Label;

        public ButtonAttribute(string label = null)
        {
            Label = label;
        }
    }
#endif
}