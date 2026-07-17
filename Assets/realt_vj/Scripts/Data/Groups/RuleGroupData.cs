using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public class RuleGroupData
    {
        [SerializeField]
        private string m_GroupName;

        [SerializeField]
        private List<string> m_MemberGuids = new();

        [SerializeField]
        private bool m_IsActive;

        public string GroupName
        {
            get => m_GroupName;
            set => m_GroupName = value;
        }

        public IReadOnlyList<string> MemberGuids => m_MemberGuids;

        public bool IsActive
        {
            get => m_IsActive;
            set => m_IsActive = value;
        }

        public RuleGroupData() { }

        public RuleGroupData(string groupName)
        {
            m_GroupName = groupName;
        }

        public void AddMember(string guid)
        {
            if (string.IsNullOrEmpty(guid) || m_MemberGuids.Contains(guid)) return;
            m_MemberGuids.Add(guid);
        }

        public bool RemoveMember(string guid)
        {
            return m_MemberGuids.Remove(guid);
        }

        public bool ContainsMember(string guid)
        {
            return m_MemberGuids.Contains(guid);
        }

        public void ClearMembers()
        {
            m_MemberGuids.Clear();
        }
    }
}
