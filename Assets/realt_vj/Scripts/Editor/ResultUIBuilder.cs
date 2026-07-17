using System;
using System.Reflection;
using RealtVJ.Data;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealtVJ.Editor
{
    public static class ResultUIBuilder
    {
        private const BindingFlags FieldFlags = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;

        public static void BuildFields(VisualElement container, Result result)
        {
            var type = result.GetType();
            var fields = type.GetFields(FieldFlags);

            foreach (var field in fields)
            {
                if (!field.IsDefined(typeof(SerializeField), false) && !field.IsPublic)
                    continue;

                // Skip the GUID field from Result base
                if (field.Name == "m_Guid") continue;

                var label = NicifyFieldName(field.Name);
                var fieldType = field.FieldType;

                if (fieldType == typeof(bool))
                    AddToggle(container, label, (bool)field.GetValue(result), v => field.SetValue(result, v));
                else if (fieldType == typeof(int))
                    AddIntField(container, label, (int)field.GetValue(result), v => field.SetValue(result, v));
                else if (fieldType == typeof(float))
                    AddFloatField(container, label, (float)field.GetValue(result), v => field.SetValue(result, v));
                else if (fieldType == typeof(string))
                    AddTextField(container, label, (string)field.GetValue(result), v => field.SetValue(result, v));
                else if (fieldType == typeof(Vector2))
                    AddVector2Field(container, label, (Vector2)field.GetValue(result), v => field.SetValue(result, v));
                else if (fieldType == typeof(Vector3))
                    AddVector3Field(container, label, (Vector3)field.GetValue(result), v => field.SetValue(result, v));
                else if (fieldType == typeof(Color))
                    AddColorField(container, label, (Color)field.GetValue(result), v => field.SetValue(result, v));
                else if (fieldType.IsEnum)
                    AddEnumField(container, label, (Enum)field.GetValue(result), v => field.SetValue(result, v));
            }
        }

        public static VisualElement CreateFieldRow(string label, VisualElement field)
        {
            var row = new VisualElement();
            row.AddToClassList("field-row");
            var lbl = new Label(label);
            lbl.AddToClassList("field-label");
            row.Add(lbl);
            field.style.flexGrow = 1;
            row.Add(field);
            return row;
        }

        public static void AddToggle(VisualElement c, string label, bool val, Action<bool> setter)
        {
            var f = new Toggle { value = val };
            f.RegisterValueChangedCallback(evt => setter(evt.newValue));
            c.Add(CreateFieldRow(label, f));
        }

        public static void AddIntField(VisualElement c, string label, int val, Action<int> setter)
        {
            var f = new IntegerField { value = val };
            f.RegisterValueChangedCallback(evt => setter(evt.newValue));
            c.Add(CreateFieldRow(label, f));
        }

        public static void AddFloatField(VisualElement c, string label, float val, Action<float> setter)
        {
            var f = new FloatField { value = val };
            f.RegisterValueChangedCallback(evt => setter(evt.newValue));
            c.Add(CreateFieldRow(label, f));
        }

        public static void AddTextField(VisualElement c, string label, string val, Action<string> setter)
        {
            var f = new TextField { value = val ?? "" };
            f.RegisterValueChangedCallback(evt => setter(evt.newValue));
            c.Add(CreateFieldRow(label, f));
        }

        public static void AddVector2Field(VisualElement c, string label, Vector2 val, Action<Vector2> setter)
        {
            var f = new Vector2Field { value = val };
            f.RegisterValueChangedCallback(evt => setter(evt.newValue));
            c.Add(CreateFieldRow(label, f));
        }

        public static void AddVector3Field(VisualElement c, string label, Vector3 val, Action<Vector3> setter)
        {
            var f = new Vector3Field { value = val };
            f.RegisterValueChangedCallback(evt => setter(evt.newValue));
            c.Add(CreateFieldRow(label, f));
        }

        public static void AddColorField(VisualElement c, string label, Color val, Action<Color> setter)
        {
            var f = new ColorField { value = val };
            f.RegisterValueChangedCallback(evt => setter(evt.newValue));
            c.Add(CreateFieldRow(label, f));
        }

        public static void AddEnumField(VisualElement c, string label, Enum val, Action<Enum> setter)
        {
            var f = new EnumField(val);
            f.RegisterValueChangedCallback(evt => setter(evt.newValue));
            c.Add(CreateFieldRow(label, f));
        }

        private static string NicifyFieldName(string fieldName)
        {
            // Strip common prefixes: m_, s_, k_
            if (fieldName.Length > 2 && fieldName[1] == '_' &&
                (fieldName[0] == 'm' || fieldName[0] == 's' || fieldName[0] == 'k'))
            {
                fieldName = fieldName.Substring(2);
            }

            // Insert spaces before capitals: "TargetPosition" -> "Target Position"
            var result = new System.Text.StringBuilder();
            for (int i = 0; i < fieldName.Length; i++)
            {
                var ch = fieldName[i];
                if (i > 0 && char.IsUpper(ch) && !char.IsUpper(fieldName[i - 1]))
                    result.Append(' ');
                result.Append(i == 0 ? char.ToUpper(ch) : ch);
            }
            return result.ToString();
        }
    }
}
