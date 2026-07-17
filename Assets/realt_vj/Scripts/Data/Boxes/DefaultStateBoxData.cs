using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public class DefaultStateBoxData : BoxData
    {
        [SerializeReference]
        private List<Result> m_DefaultResults = new();

        public IReadOnlyList<Result> DefaultResults => m_DefaultResults;

        public DefaultStateBoxData() { }

        public DefaultStateBoxData(string guid, Vector2 position) : base(guid, position) { }

        public Result AddDefaultResult(Result result)
        {
            if (result == null) return null;
            m_DefaultResults.Add(result);
            return result;
        }

        public bool RemoveDefaultResult(Result result)
        {
            if (result == null) return false;
            return m_DefaultResults.Remove(result);
        }

        public void RemoveDefaultResultAt(int index)
        {
            if (index < 0 || index >= m_DefaultResults.Count) return;
            m_DefaultResults.RemoveAt(index);
        }

        public void ReplaceDefaultResultAt(int index, Result result)
        {
            if (index < 0 || index >= m_DefaultResults.Count || result == null) return;
            m_DefaultResults[index] = result;
        }

        public void ClearDefaultResults()
        {
            m_DefaultResults.Clear();
        }
    }
}
