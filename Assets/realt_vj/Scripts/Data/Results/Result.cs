using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public abstract class Result
    {
        [SerializeField]
        private string m_Guid = System.Guid.NewGuid().ToString();

        public string Guid
        {
            get => m_Guid;
            set => m_Guid = value;
        }

        public abstract Result Clone();
    }
}
