using RealtVJ.Data;
using UnityEngine;

namespace RealtVJ.Runtime
{
    public class VolumeSettingResultExecutor : ResultExecutor<VolumeSettingResult>
    {
        [SerializeField]
        private GameObject m_TargetObject;

        protected override void Execute(VolumeSettingResult data)
        {
            if (m_TargetObject == null) return;

            // Volume/post-processing property manipulation is project-specific.
            // Override or extend this executor for your rendering pipeline.
            Debug.Log($"[VolumeSettingExecutor] Set '{data.PropertyName}' = {data.Value}, Color = {data.ColorValue} on {m_TargetObject.name}");
        }
    }
}
