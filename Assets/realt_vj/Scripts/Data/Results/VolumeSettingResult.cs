using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    [ResultInfo("Volume Setting", "Scene")]
    public sealed class VolumeSettingResult : Result
    {
        [SerializeField]
        private string m_PropertyName;

        [SerializeField]
        private float m_Value;

        [SerializeField]
        private Color m_ColorValue = Color.white;

        public string PropertyName
        {
            get => m_PropertyName;
            set => m_PropertyName = value;
        }

        public float Value
        {
            get => m_Value;
            set => m_Value = value;
        }

        public Color ColorValue
        {
            get => m_ColorValue;
            set => m_ColorValue = value;
        }

        public VolumeSettingResult() { }

        public override Result Clone()
        {
            return new VolumeSettingResult
            {
                m_PropertyName = m_PropertyName,
                m_Value = m_Value,
                m_ColorValue = m_ColorValue
            };
        }
    }
}
