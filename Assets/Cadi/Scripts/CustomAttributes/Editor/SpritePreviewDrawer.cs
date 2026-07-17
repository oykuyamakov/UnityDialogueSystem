#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace Cadi.Scripts.CustomAttributes.Editor
{
    [CustomPropertyDrawer(typeof(SpritePreviewAttribute))]
    public sealed class SpritePreviewDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var attr = (SpritePreviewAttribute)attribute;

            float h = EditorGUIUtility.singleLineHeight;

            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return h;

            var sprite = property.objectReferenceValue as Sprite;
            if (sprite == null && !attr.ShowWhenNull)
                return h;

            return h + 4f + attr.Height;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            var attr = (SpritePreviewAttribute)attribute;

            var line = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
            EditorGUI.PropertyField(line, property, label);

            if (property.propertyType != SerializedPropertyType.ObjectReference)
                return;

            var sprite = property.objectReferenceValue as Sprite;
            if (sprite == null && !attr.ShowWhenNull)
                return;

            // Preview rect under the field
            var previewRect = new Rect(
                position.x,
                line.yMax + 4f,
                position.width,
                attr.Height
            );

            DrawSpritePreview(previewRect, sprite);
        }

        private static void DrawSpritePreview(Rect rect, Sprite sprite)
        {
            // Background (helps visibility)
            EditorGUI.DrawRect(rect, new Color(0f, 0f, 0f, 0.15f));

            if (sprite == null)
            {
                EditorGUI.LabelField(rect, "None");
                return;
            }

            var tex = sprite.texture;
            if (tex == null)
            {
                EditorGUI.LabelField(rect, "No texture");
                return;
            }

            // Crop sprite rect from atlas texture
            Rect tr = sprite.textureRect;
            Rect uv = new Rect(
                tr.x / tex.width,
                tr.y / tex.height,
                tr.width / tex.width,
                tr.height / tex.height
            );

            // Keep aspect
            float aspect = tr.width / tr.height;
            Rect r = rect;

            if (r.width / r.height > aspect)
            {
                float w = r.height * aspect;
                r.x += (r.width - w) * 0.5f;
                r.width = w;
            }
            else
            {
                float h = r.width / aspect;
                r.y += (r.height - h) * 0.5f;
                r.height = h;
            }

            GUI.DrawTextureWithTexCoords(r, tex, uv, true);
        }
    }
}
#endif