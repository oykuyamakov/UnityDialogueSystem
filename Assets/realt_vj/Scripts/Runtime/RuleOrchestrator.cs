using System;
using System.Collections.Generic;
using RealtVJ.Data;
using UnityEngine;

namespace RealtVJ.Runtime
{
    public class RuleOrchestrator : MonoBehaviour
    {
        [SerializeField]
        private RuleGraphContainer m_Graph;

        [Serializable]
        public class ResultBinding
        {
            public string ResultGuid;
            public ResultExecutor Executor;
        }

        [SerializeField]
        private List<ResultBinding> m_Bindings = new();

        public RuleGraphContainer Graph => m_Graph;
        public List<ResultBinding> Bindings => m_Bindings;

        private Dictionary<string, ResultExecutor> m_ExecutorByGuid;

        private void Awake()
        {
            RebuildLookup();
        }

        public void RebuildLookup()
        {
            m_ExecutorByGuid = new Dictionary<string, ResultExecutor>();
            foreach (var binding in m_Bindings)
            {
                if (!string.IsNullOrEmpty(binding.ResultGuid) && binding.Executor != null)
                    m_ExecutorByGuid[binding.ResultGuid] = binding.Executor;
            }
        }

        public void ExecuteResult(Result result)
        {
            if (result == null) return;
            if (m_ExecutorByGuid == null) RebuildLookup();

            if (m_ExecutorByGuid.TryGetValue(result.Guid, out var executor))
                executor.Execute(result);
        }

        public void RevertResult(Result result)
        {
            if (result == null) return;
            if (m_ExecutorByGuid == null) RebuildLookup();

            if (m_ExecutorByGuid.TryGetValue(result.Guid, out var executor))
                executor.Revert();
        }

        public IEnumerable<(string guid, string typeName, string displayName)> GetAllResultEntries()
        {
            if (m_Graph == null) yield break;

            foreach (var box in m_Graph.ResultBoxes)
            {
                foreach (var result in box.Results)
                {
                    var type = result.GetType();
                    var attr = (ResultInfoAttribute)Attribute.GetCustomAttribute(type, typeof(ResultInfoAttribute));
                    var displayName = attr?.DisplayName ?? type.Name;
                    yield return (result.Guid, type.Name, displayName);
                }
            }

            foreach (var box in m_Graph.DefaultStateBoxes)
            {
                foreach (var result in box.DefaultResults)
                {
                    var type = result.GetType();
                    var attr = (ResultInfoAttribute)Attribute.GetCustomAttribute(type, typeof(ResultInfoAttribute));
                    var displayName = attr?.DisplayName ?? type.Name;
                    yield return (result.Guid, type.Name, displayName);
                }
            }
        }
    }
}
