using UnityEngine;

namespace Cadi.Scripts.Utility.Extensions
{
    public static class TransformExtensions
    {
        // -------------------------
        // World position setters
        // -------------------------

        public static void SetPositionX(this Transform transform, float x)
        {
            transform.position = transform.position.WithX(x);
        }

        public static void SetPositionY(this Transform transform, float y)
        {
            transform.position = transform.position.WithY(y);
        }

        public static void SetPositionZ(this Transform transform, float z)
        {
            transform.position = transform.position.WithZ(z);
        }

        public static void SetPositionXY(this Transform transform, float x, float y)
        {
            transform.position = transform.position.WithXY(x, y);
        }

        public static void SetPositionXZ(this Transform transform, float x, float z)
        {
            transform.position = transform.position.WithXZ(x, z);
        }

        public static void SetPositionYZ(this Transform transform, float y, float z)
        {
            transform.position = transform.position.WithYZ(y, z);
        }

        public static void SetPositionXYZ(this Transform transform, float x, float y, float z)
        {
            transform.position = new Vector3(x, y, z);
        }

        // -------------------------
        // Local position setters
        // -------------------------

        public static void SetLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition = transform.localPosition.WithX(x);
        }

        public static void SetLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = transform.localPosition.WithY(y);
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = transform.localPosition.WithZ(z);
        }

        public static void SetLocalPositionXY(this Transform transform, float x, float y)
        {
            transform.localPosition = transform.localPosition.WithXY(x, y);
        }

        public static void SetLocalPositionXZ(this Transform transform, float x, float z)
        {
            transform.localPosition = transform.localPosition.WithXZ(x, z);
        }

        public static void SetLocalPositionYZ(this Transform transform, float y, float z)
        {
            transform.localPosition = transform.localPosition.WithYZ(y, z);
        }

        public static void SetLocalPositionXYZ(this Transform transform, float x, float y, float z)
        {
            transform.localPosition = new Vector3(x, y, z);
        }

        // -------------------------
        // World position adders
        // -------------------------

        public static void AddPositionX(this Transform transform, float x)
        {
            transform.position = transform.position.AddX(x);
        }

        public static void AddPositionY(this Transform transform, float y)
        {
            transform.position = transform.position.AddY(y);
        }

        public static void AddPositionZ(this Transform transform, float z)
        {
            transform.position = transform.position.AddZ(z);
        }

        public static void AddPosition(this Transform transform, Vector3 delta)
        {
            transform.position += delta;
        }

        // -------------------------
        // Local position adders
        // -------------------------

        public static void AddLocalPositionX(this Transform transform, float x)
        {
            transform.localPosition = transform.localPosition.AddX(x);
        }

        public static void AddLocalPositionY(this Transform transform, float y)
        {
            transform.localPosition = transform.localPosition.AddY(y);
        }

        public static void AddLocalPositionZ(this Transform transform, float z)
        {
            transform.localPosition = transform.localPosition.AddZ(z);
        }

        public static void AddLocalPosition(this Transform transform, Vector3 delta)
        {
            transform.localPosition += delta;
        }

        // -------------------------
        // Local scale setters
        // -------------------------

        public static void SetScaleX(this Transform transform, float x)
        {
            transform.localScale = transform.localScale.WithX(x);
        }

        public static void SetScaleY(this Transform transform, float y)
        {
            transform.localScale = transform.localScale.WithY(y);
        }

        public static void SetScaleZ(this Transform transform, float z)
        {
            transform.localScale = transform.localScale.WithZ(z);
        }

        public static void SetScaleXY(this Transform transform, float x, float y)
        {
            transform.localScale = transform.localScale.WithXY(x, y);
        }

        public static void SetScaleXZ(this Transform transform, float x, float z)
        {
            transform.localScale = transform.localScale.WithXZ(x, z);
        }

        public static void SetScaleYZ(this Transform transform, float y, float z)
        {
            transform.localScale = transform.localScale.WithYZ(y, z);
        }

        public static void SetScaleXYZ(this Transform transform, float x, float y, float z)
        {
            transform.localScale = new Vector3(x, y, z);
        }

        // -------------------------
        // Local scale adders
        // -------------------------

        public static void AddScaleX(this Transform transform, float x)
        {
            transform.localScale = transform.localScale.AddX(x);
        }

        public static void AddScaleY(this Transform transform, float y)
        {
            transform.localScale = transform.localScale.AddY(y);
        }

        public static void AddScaleZ(this Transform transform, float z)
        {
            transform.localScale = transform.localScale.AddZ(z);
        }

        public static void AddScale(this Transform transform, Vector3 delta)
        {
            transform.localScale += delta;
        }

        // -------------------------
        // Clamped local scale adders
        // -------------------------

        public static void AddScaleXClamped(this Transform transform, float x, float min, float max)
        {
            transform.localScale = transform.localScale.AddX(x).ClampX(min, max);
        }

        public static void AddScaleYClamped(this Transform transform, float y, float min, float max)
        {
            transform.localScale = transform.localScale.AddY(y).ClampY(min, max);
        }

        public static void AddScaleZClamped(this Transform transform, float z, float min, float max)
        {
            transform.localScale = transform.localScale.AddZ(z).ClampZ(min, max);
        }

        public static void AddScaleXClamped01(this Transform transform, float x)
        {
            transform.AddScaleXClamped(x, 0f, 1f);
        }

        public static void AddScaleYClamped01(this Transform transform, float y)
        {
            transform.AddScaleYClamped(y, 0f, 1f);
        }

        public static void AddScaleZClamped01(this Transform transform, float z)
        {
            transform.AddScaleZClamped(z, 0f, 1f);
        }

        // -------------------------
        // Hierarchy helpers
        // -------------------------

        public static Transform FindDeepChild(this Transform parent, string name, bool includeSelf = false)
        {
            if (parent == null || string.IsNullOrEmpty(name))
                return null;

            if (includeSelf && parent.name == name)
                return parent;

            foreach (Transform child in parent)
            {
                if (child.name == name)
                    return child;

                Transform result = child.FindDeepChild(name);

                if (result != null)
                    return result;
            }

            return null;
        }
    }
}