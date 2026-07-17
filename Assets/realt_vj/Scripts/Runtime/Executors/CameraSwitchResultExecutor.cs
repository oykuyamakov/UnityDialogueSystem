using RealtVJ.Data;
using UnityEngine;

namespace RealtVJ.Runtime
{
    public class CameraSwitchResultExecutor : ResultExecutor<CameraSwitchResult>
    {
        [SerializeField]
        private Camera m_Camera;

        protected override void Execute(CameraSwitchResult data)
        {
            if (m_Camera == null) return;

            m_Camera.enabled = true;
            m_Camera.rect = data.ViewMode switch
            {
                CameraViewMode.Full => new Rect(0, 0, 1, 1),
                CameraViewMode.LeftHalf => new Rect(0, 0, 0.5f, 1),
                CameraViewMode.RightHalf => new Rect(0.5f, 0, 0.5f, 1),
                CameraViewMode.TopHalf => new Rect(0, 0.5f, 1, 0.5f),
                CameraViewMode.BottomHalf => new Rect(0, 0, 1, 0.5f),
                _ => new Rect(0, 0, 1, 1)
            };
        }

        public override void Revert()
        {
            if (m_Camera != null)
                m_Camera.rect = new Rect(0, 0, 1, 1);
        }
    }
}
