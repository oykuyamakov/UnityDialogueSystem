using UnityEngine;

namespace Cadi.Scripts.Utility.Extensions
{
    public static class Vector3Extensions
    {
        // -------------------------
        // Axis setters
        // -------------------------

        public static Vector3 WithX(this Vector3 vector, float x)
        {
            vector.x = x;
            return vector;
        }

        public static Vector3 WithY(this Vector3 vector, float y)
        {
            vector.y = y;
            return vector;
        }

        public static Vector3 WithZ(this Vector3 vector, float z)
        {
            vector.z = z;
            return vector;
        }

        public static Vector3 WithXY(this Vector3 vector, float x, float y)
        {
            vector.x = x;
            vector.y = y;
            return vector;
        }

        public static Vector3 WithXZ(this Vector3 vector, float x, float z)
        {
            vector.x = x;
            vector.z = z;
            return vector;
        }

        public static Vector3 WithYZ(this Vector3 vector, float y, float z)
        {
            vector.y = y;
            vector.z = z;
            return vector;
        }

        public static Vector3 WithXYZ(this Vector3 vector, float x, float y, float z)
        {
            vector.x = x;
            vector.y = y;
            vector.z = z;
            return vector;
        }

        // -------------------------
        // Axis adders
        // -------------------------

        public static Vector3 AddX(this Vector3 vector, float x)
        {
            vector.x += x;
            return vector;
        }

        public static Vector3 AddY(this Vector3 vector, float y)
        {
            vector.y += y;
            return vector;
        }

        public static Vector3 AddZ(this Vector3 vector, float z)
        {
            vector.z += z;
            return vector;
        }

        public static Vector3 AddXY(this Vector3 vector, float x, float y)
        {
            vector.x += x;
            vector.y += y;
            return vector;
        }

        public static Vector3 AddXZ(this Vector3 vector, float x, float z)
        {
            vector.x += x;
            vector.z += z;
            return vector;
        }

        public static Vector3 AddYZ(this Vector3 vector, float y, float z)
        {
            vector.y += y;
            vector.z += z;
            return vector;
        }

        // -------------------------
        // Axis multipliers
        // -------------------------

        public static Vector3 MultiplyX(this Vector3 vector, float x)
        {
            vector.x *= x;
            return vector;
        }

        public static Vector3 MultiplyY(this Vector3 vector, float y)
        {
            vector.y *= y;
            return vector;
        }

        public static Vector3 MultiplyZ(this Vector3 vector, float z)
        {
            vector.z *= z;
            return vector;
        }

        public static Vector3 MultiplyXY(this Vector3 vector, float x, float y)
        {
            vector.x *= x;
            vector.y *= y;
            return vector;
        }

        public static Vector3 MultiplyXZ(this Vector3 vector, float x, float z)
        {
            vector.x *= x;
            vector.z *= z;
            return vector;
        }

        public static Vector3 MultiplyYZ(this Vector3 vector, float y, float z)
        {
            vector.y *= y;
            vector.z *= z;
            return vector;
        }

        // -------------------------
        // Zero / flatten helpers
        // -------------------------

        public static Vector3 WithZeroX(this Vector3 vector)
        {
            vector.x = 0f;
            return vector;
        }

        public static Vector3 WithZeroY(this Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }

        public static Vector3 WithZeroZ(this Vector3 vector)
        {
            vector.z = 0f;
            return vector;
        }

        public static Vector3 FlattenX(this Vector3 vector)
        {
            vector.x = 0f;
            return vector;
        }

        public static Vector3 FlattenY(this Vector3 vector)
        {
            vector.y = 0f;
            return vector;
        }

        public static Vector3 FlattenZ(this Vector3 vector)
        {
            vector.z = 0f;
            return vector;
        }

        // -------------------------
        // Clamp helpers
        // -------------------------

        public static Vector3 ClampX(this Vector3 vector, float min, float max)
        {
            vector.x = Mathf.Clamp(vector.x, min, max);
            return vector;
        }

        public static Vector3 ClampY(this Vector3 vector, float min, float max)
        {
            vector.y = Mathf.Clamp(vector.y, min, max);
            return vector;
        }

        public static Vector3 ClampZ(this Vector3 vector, float min, float max)
        {
            vector.z = Mathf.Clamp(vector.z, min, max);
            return vector;
        }

        public static Vector3 Clamp(this Vector3 vector, Vector3 min, Vector3 max)
        {
            vector.x = Mathf.Clamp(vector.x, min.x, max.x);
            vector.y = Mathf.Clamp(vector.y, min.y, max.y);
            vector.z = Mathf.Clamp(vector.z, min.z, max.z);
            return vector;
        }

        // -------------------------
        // Absolute / sign helpers
        // -------------------------

        public static Vector3 Abs(this Vector3 vector)
        {
            vector.x = Mathf.Abs(vector.x);
            vector.y = Mathf.Abs(vector.y);
            vector.z = Mathf.Abs(vector.z);
            return vector;
        }

        public static Vector3 Sign(this Vector3 vector)
        {
            vector.x = SignZeroAware(vector.x);
            vector.y = SignZeroAware(vector.y);
            vector.z = SignZeroAware(vector.z);
            return vector;
        }

        private static float SignZeroAware(float value)
        {
            if (value > 0f)
                return 1f;

            if (value < 0f)
                return -1f;

            return 0f;
        }

        // -------------------------
        // Conversions
        // -------------------------

        public static Vector2 ToXY(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.y);
        }

        public static Vector2 ToXZ(this Vector3 vector)
        {
            return new Vector2(vector.x, vector.z);
        }

        public static Vector2 ToYZ(this Vector3 vector)
        {
            return new Vector2(vector.y, vector.z);
        }

        public static Vector3Int RoundToInt(this Vector3 vector)
        {
            return new Vector3Int(
                Mathf.RoundToInt(vector.x),
                Mathf.RoundToInt(vector.y),
                Mathf.RoundToInt(vector.z)
            );
        }

        public static Vector3Int FloorToInt(this Vector3 vector)
        {
            return new Vector3Int(
                Mathf.FloorToInt(vector.x),
                Mathf.FloorToInt(vector.y),
                Mathf.FloorToInt(vector.z)
            );
        }

        public static Vector3Int CeilToInt(this Vector3 vector)
        {
            return new Vector3Int(
                Mathf.CeilToInt(vector.x),
                Mathf.CeilToInt(vector.y),
                Mathf.CeilToInt(vector.z)
            );
        }

        // -------------------------
        // Distance helpers
        // -------------------------

        public static float DistanceXZ(this Vector3 from, Vector3 to)
        {
            return Mathf.Sqrt(SqrDistanceXZ(from, to));
        }

        public static float SqrDistanceXZ(this Vector3 from, Vector3 to)
        {
            float dx = to.x - from.x;
            float dz = to.z - from.z;
            return dx * dx + dz * dz;
        }

        public static Vector3 DirectionTo(this Vector3 from, Vector3 to)
        {
            return (to - from).SafeNormalized();
        }

        public static Vector3 DirectionToXZ(this Vector3 from, Vector3 to)
        {
            Vector3 direction = new Vector3(to.x - from.x, 0f, to.z - from.z);
            return direction.SafeNormalized();
        }

        // -------------------------
        // Approx helpers
        // -------------------------

        public static bool Approximately(this Vector3 a, Vector3 b, float tolerance = 0.0001f)
        {
            tolerance = Mathf.Abs(tolerance);
            return (a - b).sqrMagnitude <= tolerance * tolerance;
        }

        // -------------------------
        // Safe normalization
        // -------------------------

        public static Vector3 SafeNormalized(this Vector3 vector, float minSqrMagnitude = 0.000001f)
        {
            if (vector.sqrMagnitude <= minSqrMagnitude)
                return Vector3.zero;

            return vector.normalized;
        }
    }
}