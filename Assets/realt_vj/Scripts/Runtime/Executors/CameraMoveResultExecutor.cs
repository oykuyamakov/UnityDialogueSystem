using System.Collections;
using RealtVJ.Data;
using UnityEngine;

namespace RealtVJ.Runtime
{
    public class CameraMoveResultExecutor : ResultExecutor<CameraMoveResult>
    {
        [SerializeField]
        private Transform m_CameraTransform;

        private Coroutine m_ActiveMove;

        protected override void Execute(CameraMoveResult data)
        {
            if (m_CameraTransform == null) return;

            if (m_ActiveMove != null)
                StopCoroutine(m_ActiveMove);

            if (data.Duration <= 0f)
            {
                m_CameraTransform.localPosition = data.TargetPosition;
                m_CameraTransform.localEulerAngles = data.TargetRotation;
            }
            else
            {
                m_ActiveMove = StartCoroutine(MoveCoroutine(data));
            }
        }

        private IEnumerator MoveCoroutine(CameraMoveResult data)
        {
            var startPos = m_CameraTransform.localPosition;
            var startRot = m_CameraTransform.localEulerAngles;
            float elapsed = 0f;

            while (elapsed < data.Duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / data.Duration);
                m_CameraTransform.localPosition = Vector3.Lerp(startPos, data.TargetPosition, t);
                m_CameraTransform.localEulerAngles = Vector3.Lerp(startRot, data.TargetRotation, t);
                yield return null;
            }

            m_CameraTransform.localPosition = data.TargetPosition;
            m_CameraTransform.localEulerAngles = data.TargetRotation;
            m_ActiveMove = null;
        }

        public override void Revert()
        {
            if (m_ActiveMove != null)
            {
                StopCoroutine(m_ActiveMove);
                m_ActiveMove = null;
            }
        }
    }
}
