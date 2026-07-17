using System;
using System.Collections.Generic;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public class ResultBoxData : BoxData
    {
        [SerializeReference]
        private List<Result> m_Results = new();

        [SerializeField]
        private ResultExecutionMode m_ExecutionMode = ResultExecutionMode.ATST;

        [SerializeField]
        private int m_LoopCount = -1;

        public IReadOnlyList<Result> Results => m_Results;

        public ResultExecutionMode ExecutionMode
        {
            get => m_ExecutionMode;
            set => m_ExecutionMode = value;
        }

        /// <summary>
        /// -1 = infinite, N = execute N times.
        /// </summary>
        public int LoopCount
        {
            get => m_LoopCount;
            set => m_LoopCount = value;
        }

        public ResultBoxData() { }

        public ResultBoxData(string guid, Vector2 position) : base(guid, position) { }

        public Result AddResult(Result result)
        {
            if (result == null) return null;
            m_Results.Add(result);
            return result;
        }

        public bool RemoveResult(Result result)
        {
            if (result == null) return false;
            return m_Results.Remove(result);
        }

        public void RemoveResultAt(int index)
        {
            if (index < 0 || index >= m_Results.Count) return;
            m_Results.RemoveAt(index);
        }

        public void ReplaceResultAt(int index, Result result)
        {
            if (index < 0 || index >= m_Results.Count || result == null) return;
            m_Results[index] = result;
        }

        public void ClearResults()
        {
            m_Results.Clear();
        }
    }
}
