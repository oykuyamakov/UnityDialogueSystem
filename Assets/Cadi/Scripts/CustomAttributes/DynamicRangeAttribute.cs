using UnityEngine;

namespace Cadi.Scripts.CustomAttributes
{
    public class DynamicRangeAttribute : PropertyAttribute
    {
        public readonly string MinField;
        public readonly string MaxField;

        public readonly float? MinConst;
        public readonly float? MaxConst;

        // field-field
        public DynamicRangeAttribute(string minField, string maxField)
        {
            MinField = minField;
            MaxField = maxField;
        }

        // field-const
        public DynamicRangeAttribute(string minField, float maxConst)
        {
            MinField = minField;
            MaxConst = maxConst;
        }

        // const-field
        public DynamicRangeAttribute(float minConst, string maxField)
        {
            MinConst = minConst;
            MaxField = maxField;
        }

        // const-const
        public DynamicRangeAttribute(float minConst, float maxConst)
        {
            MinConst = minConst;
            MaxConst = maxConst;
        }
    }
}