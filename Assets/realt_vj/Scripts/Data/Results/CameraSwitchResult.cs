using System;
using UnityEngine;

namespace RealtVJ.Data
{
    public enum CameraViewMode
    {
        Full,
        LeftHalf,
        RightHalf,
        TopHalf,
        BottomHalf
    }

    [Serializable]
    [ResultInfo("Camera Switch", "Camera")]
    public sealed class CameraSwitchResult : Result
    {
        [SerializeField]
        private CameraViewMode m_ViewMode = CameraViewMode.Full;

        public CameraViewMode ViewMode
        {
            get => m_ViewMode;
            set => m_ViewMode = value;
        }

        public CameraSwitchResult() { }

        public CameraSwitchResult(CameraViewMode viewMode)
        {
            m_ViewMode = viewMode;
        }

        public override Result Clone()
        {
            return new CameraSwitchResult(m_ViewMode);
        }
    }
}
