using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    [ResultInfo("Object", "Scene")]
    public sealed class ObjectResult : Result
    {
        [SerializeField]
        private bool m_SetActive = true;

        [SerializeField]
        private Vector3 m_Scale = Vector3.one;

        [SerializeField]
        private Vector3 m_Rotation;

        [SerializeField]
        private Vector3 m_Position;

        public bool SetActive
        {
            get => m_SetActive;
            set => m_SetActive = value;
        }

        public Vector3 Scale
        {
            get => m_Scale;
            set => m_Scale = value;
        }

        public Vector3 Rotation
        {
            get => m_Rotation;
            set => m_Rotation = value;
        }

        public Vector3 Position
        {
            get => m_Position;
            set => m_Position = value;
        }

        public ObjectResult() { }

        public override Result Clone()
        {
            return new ObjectResult
            {
                m_SetActive = m_SetActive,
                m_Scale = m_Scale,
                m_Rotation = m_Rotation,
                m_Position = m_Position
            };
        }
    }
}
