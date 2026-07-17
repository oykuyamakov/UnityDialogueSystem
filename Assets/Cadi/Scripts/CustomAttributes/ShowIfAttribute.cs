using System;
using UnityEngine;

namespace Cadi.Scripts.CustomAttributes
{
    public enum ShowIfComparison
    {
        IsTrue, // bool member true i
        Equals, // member == value
        NotEquals // member != value
    }
    
    public sealed class ShowIfNotEqualsAttribute : ShowIfAttribute
    {
        public ShowIfNotEqualsAttribute(string conditionMember, int value)
            : base(conditionMember, value, ShowIfComparison.NotEquals)
        {
        }
    }


    [AttributeUsage(AttributeTargets.Field, AllowMultiple = true, Inherited = true)]
    public  class ShowIfAttribute : PropertyAttribute
    {
        public readonly string ConditionMember;
        public readonly ShowIfComparison Comparison;

        // Expected values (attribute param restrictions so seperated)
        public readonly int IntValue;
        public readonly float FloatValue;
        public readonly string StringValue;
        public readonly bool BoolValue;

        public readonly bool HasInt;
        public readonly bool HasFloat;
        public readonly bool HasString;
        public readonly bool HasBool;

        // 1) bool condition: [ShowIf("SomeBool")]
        public ShowIfAttribute(string conditionMember)
        {
            ConditionMember = conditionMember;
            Comparison = ShowIfComparison.IsTrue;
        }

        // 2) enum/int compare: [ShowIf("SomeEnum", (int)MyEnum.Value, ShowIfComparison.Equals/NotEquals)]
        public ShowIfAttribute(string conditionMember, int value, ShowIfComparison comparison = ShowIfComparison.Equals)
        {
            ConditionMember = conditionMember;
            Comparison = comparison;

            IntValue = value;
            HasInt = true;
        }

        // 3) bool compare (rare): [ShowIf("SomeBool", true, ShowIfComparison.Equals)]
        public ShowIfAttribute(string conditionMember, bool value,
            ShowIfComparison comparison = ShowIfComparison.Equals)
        {
            ConditionMember = conditionMember;
            Comparison = comparison;

            BoolValue = value;
            HasBool = true;
        }

        // 4) float compare: [ShowIf("SomeFloat", 0.5f, ShowIfComparison.Equals)]
        public ShowIfAttribute(string conditionMember, float value,
            ShowIfComparison comparison = ShowIfComparison.Equals)
        {
            ConditionMember = conditionMember;
            Comparison = comparison;

            FloatValue = value;
            HasFloat = true;
        }

        // 5) string compare: [ShowIf("SomeString", "abc", ShowIfComparison.Equals)]
        public ShowIfAttribute(string conditionMember, string value,
            ShowIfComparison comparison = ShowIfComparison.Equals)
        {
            ConditionMember = conditionMember;
            Comparison = comparison;

            StringValue = value;
            HasString = true;
        }
    }
}