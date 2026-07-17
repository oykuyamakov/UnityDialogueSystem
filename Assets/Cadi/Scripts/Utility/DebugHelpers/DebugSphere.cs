using UnityEngine;

namespace Cadi.Scripts.Utility.DebugHelpers
{
    [ExecuteInEditMode]
    public class DebugSphere : MonoBehaviour
    {
#if UNITY_EDITOR

        public float Radius = 0.5f;

        public Color Color = Color.red;

        public bool OnlyWhenSelected = false;

        private void OnDrawGizmos()
        {
            if (!OnlyWhenSelected)
            {
                Gizmos.color = Color;
                Gizmos.DrawSphere(transform.position, Radius);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (OnlyWhenSelected)
            {
                Gizmos.color = Color;
                Gizmos.DrawSphere(transform.position, Radius);
            }
        }

#endif
    }
}