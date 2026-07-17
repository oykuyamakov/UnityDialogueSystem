using System;

namespace RealtVJ.Data
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ResultInfoAttribute : Attribute
    {
        public string DisplayName { get; }
        public string Category { get; }

        public ResultInfoAttribute(string displayName, string category = "General")
        {
            DisplayName = displayName;
            Category = category;
        }
    }
}
