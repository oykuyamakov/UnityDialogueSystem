using RealtVJ.Data;
using UnityEngine;

namespace RealtVJ.Runtime
{
    public class ObjectResultExecutor : ResultExecutor<ObjectResult>
    {
        [SerializeField]
        private GameObject m_TargetObject;

        private bool m_PreviousActive;
        private Vector3 m_PreviousPosition;
        private Vector3 m_PreviousRotation;
        private Vector3 m_PreviousScale;

        protected override void Execute(ObjectResult data)
        {
            if (m_TargetObject == null) return;

            // Store previous state for revert
            m_PreviousActive = m_TargetObject.activeSelf;
            m_PreviousPosition = m_TargetObject.transform.localPosition;
            m_PreviousRotation = m_TargetObject.transform.localEulerAngles;
            m_PreviousScale = m_TargetObject.transform.localScale;

            m_TargetObject.SetActive(data.SetActive);
            m_TargetObject.transform.localPosition = data.Position;
            m_TargetObject.transform.localEulerAngles = data.Rotation;
            m_TargetObject.transform.localScale = data.Scale;
        }

        public override void Revert()
        {
            if (m_TargetObject == null) return;

            m_TargetObject.SetActive(m_PreviousActive);
            m_TargetObject.transform.localPosition = m_PreviousPosition;
            m_TargetObject.transform.localEulerAngles = m_PreviousRotation;
            m_TargetObject.transform.localScale = m_PreviousScale;
        }
    }
}
