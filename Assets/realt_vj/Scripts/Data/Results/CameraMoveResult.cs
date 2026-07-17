using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    [ResultInfo("Camera Move", "Camera")]
    public sealed class CameraMoveResult : Result
    {
        [SerializeField]
        private Vector3 m_TargetPosition;

        [SerializeField]
        private Vector3 m_TargetRotation;

        [SerializeField]
        private float m_Duration = 1f;

        public Vector3 TargetPosition
        {
            get => m_TargetPosition;
            set => m_TargetPosition = value;
        }

        public Vector3 TargetRotation
        {
            get => m_TargetRotation;
            set => m_TargetRotation = value;
        }

        public float Duration
        {
            get => m_Duration;
            set => m_Duration = value;
        }

        public CameraMoveResult() { }

        public CameraMoveResult(Vector3 targetPosition, Vector3 targetRotation, float duration = 1f)
        {
            m_TargetPosition = targetPosition;
            m_TargetRotation = targetRotation;
            m_Duration = duration;
        }

        public override Result Clone()
        {
            return new CameraMoveResult(m_TargetPosition, m_TargetRotation, m_Duration);
        }
    }
}
