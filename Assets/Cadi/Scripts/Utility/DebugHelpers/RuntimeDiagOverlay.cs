using System.Text;
using UnityEngine;

namespace Cadi.Scripts.Utility.DebugHelpers
{
    public sealed class RuntimeDiagOverlay : MonoBehaviour
    {
        [SerializeField]
        private bool m_Show = true;

        private float m_Fps;
        private float m_FpsTimer;
        private int m_FpsFrames;

        private float m_LastTs;
        private float m_LastNewFrameRealtime;

        private readonly StringBuilder m_Sb = new(512);

        private GUIStyle m_BoxStyle;


        private void Update()
        {
            if (!m_Show)
                return;

            // Unity FPS
            m_FpsFrames++;
            m_FpsTimer += Time.unscaledDeltaTime;
            if (m_FpsTimer >= 0.5f)
            {
                m_Fps = m_FpsFrames / m_FpsTimer;
                m_FpsFrames = 0;
                m_FpsTimer = 0f;
            }


            if (Input.GetKeyDown(KeyCode.F1))
                m_Show = !m_Show;
        }

        private void OnGUI()
        {
            if (!m_Show)
                return;

            m_BoxStyle = new GUIStyle(GUI.skin.box);
            m_BoxStyle.fontSize = 25;
            m_BoxStyle.alignment = TextAnchor.UpperLeft;
            m_BoxStyle.wordWrap = false;


            float frameAgeMs = (Time.realtimeSinceStartup - m_LastNewFrameRealtime) * 1000f;

            m_Sb.Clear();
            m_Sb.AppendLine("=== MotionTrack Diagnostics (F1 toggle) ===");
            m_Sb.AppendLine(
                $"Unity FPS: {m_Fps:0.0}   Target: {Application.targetFrameRate}   VSync: {QualitySettings.vSyncCount}");
            m_Sb.AppendLine(
                $"Screen: {Screen.width}x{Screen.height}  Refresh: {Screen.currentResolution.refreshRateRatio.value:0.0} Hz  Fullscreen: {Screen.fullScreenMode}");
            m_Sb.AppendLine(
                $"Device: {SystemInfo.deviceName}  GPU: {SystemInfo.graphicsDeviceName} ({SystemInfo.graphicsDeviceType})");

            GUI.color = Color.white;
            GUI.Box(
                new Rect(10, 10, 560 * 1.2f, 210 * 1.2f),
                m_Sb.ToString(),
                m_BoxStyle
            );
        }
    }
}