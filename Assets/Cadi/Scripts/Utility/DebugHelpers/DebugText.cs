using UnityEditor;
using UnityEngine;

namespace Cadi.Scripts.Utility.DebugHelpers
{
    [ExecuteAlways]
    public class DebugText : MonoBehaviour
    {
        public enum TextMode
        {
            None,
            ObjectName,
            OverrideName,
            Transform
        }

        [Header("Mode")]
        public TextMode Mode = TextMode.ObjectName;

        [Tooltip("Used only when mode=OverrideName. If empty/whitespace, it falls back to ObjectName.")]
        public string OverrideName = "";

        [Tooltip(
            "When printing ObjectName/OverrideName, remove this substring before drawing (example: 'mixamorig:').")]
        public string RemoveSubstring = "";

        [Header("Appearance")]
        public Color Color = Color.white;

        [Min(6)]
        public int FontSize = 12;

        [Tooltip("World-space offset for label position.")]
        public Vector3 WorldOffset = new Vector3(0f, 0.03f, 0f);

        [Header("Transform Formatting")]
        [Min(0)]
        public int Decimals = 2;

        [Tooltip("If true, label is drawn only when this object is selected (Scene view).")]
        public bool OnlyWhenSelected = false;

        public bool BoneEnabled;

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Draw(false);
        }

        private void OnDrawGizmosSelected()
        {
            Draw(true);
        }

        private void Draw(bool selectedPass)
        {
            if (Mode == TextMode.None)
            {
                return;
            }

            if (OnlyWhenSelected && !selectedPass)
            {
                return;
            }

            if (!BoneEnabled)
            {
                return;
            }

            string text = BuildText();
            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            GUIStyle style = new GUIStyle(EditorStyles.boldLabel);
            style.fontSize = FontSize;
            style.normal.textColor = Color;

            Vector3 pos = transform.position + WorldOffset;
            Handles.Label(pos, text, style);
        }
#endif

        private string BuildText()
        {
            switch (Mode)
            {
                case TextMode.ObjectName:
                case TextMode.OverrideName when string.IsNullOrWhiteSpace(OverrideName):
                    return CleanName(gameObject.name);
                case TextMode.OverrideName:
                    return CleanName(OverrideName);
                case TextMode.Transform:
                {
                    Vector3 p = transform.position;
                    Vector3 s = transform.lossyScale;
                    Vector3 r = transform.eulerAngles;

                    int d = Mathf.Clamp(Decimals, 0, 6);

                    string ps = $"p: ({p.x.ToString($"F{d}")}, {p.y.ToString($"F{d}")}, {p.z.ToString($"F{d}")})";
                    string rs = $"r: ({r.x.ToString($"F{d}")}, {r.y.ToString($"F{d}")}, {r.z.ToString($"F{d}")})";
                    string ss = $"s: ({s.x.ToString($"F{d}")}, {s.y.ToString($"F{d}")}, {s.z.ToString($"F{d}")})";

                    return $"{ps}\n{rs}\n{ss}";
                }
                default:
                    return "";
            }
        }

        private string CleanName(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return "";
            }

            if (!string.IsNullOrEmpty(RemoveSubstring))
            {
                value = value.Replace(RemoveSubstring, "");
            }

            return value.Trim();
        }
    }
}