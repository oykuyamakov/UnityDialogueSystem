#if UNITY_EDITOR
using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace Cadi.Scripts.CustomAttributes.Editor
{
    [CustomPropertyDrawer(typeof(ShowIfAttribute), true)]
    public class ShowIfDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            if (!ShouldShow(property))
            {
                return -EditorGUIUtility.standardVerticalSpacing;
            }

            var range = fieldInfo != null ? fieldInfo.GetCustomAttribute<RangeAttribute>(true) : null;
            if (range != null && (property.propertyType == SerializedPropertyType.Integer ||
                                  property.propertyType == SerializedPropertyType.Float))
            {
                return EditorGUIUtility.singleLineHeight;
            }

            return EditorGUI.GetPropertyHeight(property, label, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (!ShouldShow(property))
            {
                return;
            }

            var range = fieldInfo != null ? fieldInfo.GetCustomAttribute<RangeAttribute>(true) : null;

            if (range != null)
            {
                if (property.propertyType == SerializedPropertyType.Integer)
                {
                    EditorGUI.BeginProperty(position, label, property);
                    EditorGUI.BeginChangeCheck();
                    int v = EditorGUI.IntSlider(position, label, property.intValue,
                        Mathf.RoundToInt(range.min), Mathf.RoundToInt(range.max));
                    if (EditorGUI.EndChangeCheck())
                        property.intValue = v;
                    EditorGUI.EndProperty();
                    return;
                }

                if (property.propertyType == SerializedPropertyType.Float)
                {
                    EditorGUI.BeginProperty(position, label, property);
                    EditorGUI.BeginChangeCheck();
                    float v = EditorGUI.Slider(position, label, property.floatValue, range.min, range.max);
                    if (EditorGUI.EndChangeCheck())
                        property.floatValue = v;
                    EditorGUI.EndProperty();
                    return;
                }
            }

            EditorGUI.PropertyField(position, property, label, true);
        }

        private bool ShouldShow(SerializedProperty property)
        {
            var attr = (ShowIfAttribute)attribute;

            if (TryEvaluateFromSerialized(property, attr, out bool serializedResult))
            {
                return serializedResult;
            }

            object owner = property.serializedObject.targetObject;
            if (owner == null)
            {
                return true;
            }

            return EvaluateFromReflection(owner, attr);
        }

        private bool TryEvaluateFromSerialized(SerializedProperty property, ShowIfAttribute attr, out bool result)
        {
            result = true;

            if (property == null || property.serializedObject == null)
            {
                return false;
            }

            string propertyPath = property.propertyPath;
            int lastDot = propertyPath.LastIndexOf('.');
            string prefix = lastDot >= 0 ? propertyPath.Substring(0, lastDot + 1) : string.Empty;

            string conditionPath = prefix + attr.ConditionMember;
            var conditionProp = property.serializedObject.FindProperty(conditionPath);

            if (conditionProp == null)
            {
                conditionProp = property.serializedObject.FindProperty(attr.ConditionMember);
            }

            if (conditionProp == null)
            {
                return false;
            }

            if (attr.Comparison == ShowIfComparison.IsTrue)
            {
                if (conditionProp.propertyType != SerializedPropertyType.Boolean)
                {
                    return false;
                }

                result = conditionProp.boolValue;
                return true;
            }

            // Equals / NotEquals
            bool eq;
            if (!TrySerializedEquals(conditionProp, attr, out eq))
            {
                return false;
            }

            if (attr.Comparison == ShowIfComparison.Equals)
            {
                result = eq;
                return true;
            }

            if (attr.Comparison == ShowIfComparison.NotEquals)
            {
                result = !eq;
                return true;
            }

            return false;
        }

        private bool TrySerializedEquals(SerializedProperty conditionProp, ShowIfAttribute attr, out bool equals)
        {
            equals = false;

            switch (conditionProp.propertyType)
            {
                case SerializedPropertyType.Enum:
                {
                    if (!attr.HasInt)
                    {
                        return false;
                    }

                    // enumValueIndex not; intValue = underlying enum value
                    equals = conditionProp.intValue == attr.IntValue;
                    return true;
                }

                case SerializedPropertyType.Integer:
                {
                    if (!attr.HasInt)
                    {
                        return false;
                    }

                    equals = conditionProp.intValue == attr.IntValue;
                    return true;
                }

                case SerializedPropertyType.Boolean:
                {
                    if (!attr.HasBool)
                    {
                        return false;
                    }

                    equals = conditionProp.boolValue == attr.BoolValue;
                    return true;
                }

                case SerializedPropertyType.Float:
                {
                    if (!attr.HasFloat)
                    {
                        return false;
                    }

                    equals = Mathf.Approximately(conditionProp.floatValue, attr.FloatValue);
                    return true;
                }

                case SerializedPropertyType.String:
                {
                    if (!attr.HasString)
                    {
                        return false;
                    }

                    equals = string.Equals(conditionProp.stringValue, attr.StringValue, StringComparison.Ordinal);
                    return true;
                }
            }

            return false;
        }

        private bool EvaluateFromReflection(object owner, ShowIfAttribute attr)
        {
            var type = owner.GetType();
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            object value = null;

            var field = type.GetField(attr.ConditionMember, flags);
            if (field != null)
            {
                value = field.GetValue(owner);
            }
            else
            {
                var prop = type.GetProperty(attr.ConditionMember, flags);
                if (prop != null && prop.GetIndexParameters().Length == 0)
                {
                    value = prop.GetValue(owner);
                }
                else
                {
                    var method = type.GetMethod(attr.ConditionMember, flags);
                    if (method != null && method.GetParameters().Length == 0)
                    {
                        value = method.Invoke(owner, null);
                    }
                }
            }

            if (value == null)
            {
                return true;
            }

            if (attr.Comparison == ShowIfComparison.IsTrue)
            {
                if (value is bool b)
                {
                    return b;
                }

                return true;
            }

            bool eq;
            if (!TryReflectionEquals(value, attr, out eq))
            {
                return true;
            }

            if (attr.Comparison == ShowIfComparison.Equals)
            {
                return eq;
            }

            if (attr.Comparison == ShowIfComparison.NotEquals)
            {
                return !eq;
            }

            return true;
        }

        private bool TryReflectionEquals(object value, ShowIfAttribute attr, out bool equals)
        {
            equals = false;

            if (value is Enum e)
            {
                if (!attr.HasInt)
                {
                    return false;
                }

                equals = Convert.ToInt32(e) == attr.IntValue;
                return true;
            }

            if (value is int i)
            {
                if (!attr.HasInt)
                {
                    return false;
                }

                equals = i == attr.IntValue;
                return true;
            }

            if (value is bool b)
            {
                if (!attr.HasBool)
                {
                    return false;
                }

                equals = b == attr.BoolValue;
                return true;
            }

            if (value is float f)
            {
                if (!attr.HasFloat)
                {
                    return false;
                }

                equals = Mathf.Approximately(f, attr.FloatValue);
                return true;
            }

            if (value is string s)
            {
                if (!attr.HasString)
                {
                    return false;
                }

                equals = string.Equals(s, attr.StringValue, StringComparison.Ordinal);
                return true;
            }

            return false;
        }
    }
}
#endif
