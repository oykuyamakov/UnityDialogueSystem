using UnityEngine;

namespace Cadi.Scripts.CustomAttributes
{
    public sealed class SpritePreviewAttribute : PropertyAttribute
    {
        public readonly float Height;
        public readonly bool ShowWhenNull;

        public SpritePreviewAttribute(float height = 64f, bool showWhenNull = false)
        {
            Height = height;
            ShowWhenNull = showWhenNull;
        }
    }
}