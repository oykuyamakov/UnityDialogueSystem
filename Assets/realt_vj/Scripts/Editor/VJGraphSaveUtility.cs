using System.Collections.Generic;
using System.Linq;
using RealtVJ.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace RealtVJ.Editor
{
    public class VJGraphSaveUtility
    {
        private VJGraphView m_GraphView;
        private RuleGraphContainer m_ContainerCache;

        public static VJGraphSaveUtility GetInstance(VJGraphView graphView)
        {
            return new VJGraphSaveUtility { m_GraphView = graphView };
        }

        public void SaveGraph(string fileName)
        {
            var container = ScriptableObject.CreateInstance<RuleGraphContainer>();

            foreach (var node in m_GraphView.GetTriggerBoxNodes())
            {
                node.Data.GraphPosition = node.GetPosition().position;
                container.TriggerBoxes.Add(node.Data);
            }

            foreach (var node in m_GraphView.GetResultBoxNodes())
            {
                node.Data.GraphPosition = node.GetPosition().position;
                container.ResultBoxes.Add(node.Data);
            }

            foreach (var node in m_GraphView.GetDefaultStateNodes())
            {
                node.Data.GraphPosition = node.GetPosition().position;
                container.DefaultStateBoxes.Add(node.Data);
            }

            foreach (var edge in m_GraphView.edges.Where(e => e.input != null && e.output != null))
            {
                var outputNode = edge.output.node as VJNode;
                var inputNode = edge.input.node as VJNode;
                if (outputNode == null || inputNode == null) continue;

                var edgeType = DetermineEdgeType(outputNode, inputNode, edge.output);
                var edgeData = new EdgeData(outputNode.GUID, inputNode.GUID, edgeType);
                container.Edges.Add(edgeData);
            }

            foreach (var group in m_GraphView.graphElements.OfType<Group>())
            {
                var groupData = new RuleGroupData(group.title);
                foreach (var element in group.containedElements.OfType<VJNode>())
                    groupData.AddMember(element.GUID);

                if (groupData.MemberGuids.Count > 0)
                    container.RuleGroups.Add(groupData);
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
                AssetDatabase.CreateFolder("Assets", "Resources");
            if (!AssetDatabase.IsValidFolder("Assets/Resources/VJGraphs"))
                AssetDatabase.CreateFolder("Assets/Resources", "VJGraphs");

            AssetDatabase.CreateAsset(container, $"Assets/Resources/VJGraphs/{fileName}.asset");
            AssetDatabase.SaveAssets();
        }

        public void LoadGraph(string fileName)
        {
            m_ContainerCache = Resources.Load<RuleGraphContainer>($"VJGraphs/{fileName}");

            if (m_ContainerCache == null)
            {
                EditorUtility.DisplayDialog("File Not Found",
                    $"No graph found at Resources/VJGraphs/{fileName}", "OK");
                return;
            }

            ClearGraph();
            CreateNodes();
            CreateEdges();
            CreateGroups();
        }

        private void ClearGraph()
        {
            foreach (var element in m_GraphView.graphElements.ToList())
                m_GraphView.RemoveElement(element);
        }

        private void CreateNodes()
        {
            foreach (var data in m_ContainerCache.TriggerBoxes)
            {
                var node = m_GraphView.CreateTriggerBoxNode(data.GraphPosition, data);
                m_GraphView.AddElement(node);
            }

            foreach (var data in m_ContainerCache.ResultBoxes)
            {
                var node = m_GraphView.CreateResultBoxNode(data.GraphPosition, data);
                m_GraphView.AddElement(node);
            }

            foreach (var data in m_ContainerCache.DefaultStateBoxes)
            {
                var node = m_GraphView.CreateDefaultStateNode(data.GraphPosition, data);
                m_GraphView.AddElement(node);
            }
        }

        private void CreateEdges()
        {
            var allNodes = m_GraphView.nodes.OfType<VJNode>().ToDictionary(n => n.GUID);

            foreach (var edgeData in m_ContainerCache.Edges)
            {
                if (!allNodes.TryGetValue(edgeData.SourceGuid, out var sourceNode)) continue;
                if (!allNodes.TryGetValue(edgeData.TargetGuid, out var targetNode)) continue;

                Port outputPort = GetOutputPort(sourceNode, edgeData.EdgeType);
                Port inputPort = GetInputPort(targetNode);

                if (outputPort == null || inputPort == null) continue;

                var edge = new Edge { output = outputPort, input = inputPort };
                edge.input.Connect(edge);
                edge.output.Connect(edge);
                m_GraphView.Add(edge);
            }
        }

        private void CreateGroups()
        {
            var allNodes = m_GraphView.nodes.OfType<VJNode>().ToDictionary(n => n.GUID);

            foreach (var groupData in m_ContainerCache.RuleGroups)
            {
                var group = new Group { title = groupData.GroupName };
                group.AddToClassList("group-node");

                foreach (var memberGuid in groupData.MemberGuids)
                {
                    if (allNodes.TryGetValue(memberGuid, out var node))
                        group.AddElement(node);
                }

                m_GraphView.AddElement(group);
            }
        }

        private EdgeType DetermineEdgeType(VJNode sourceNode, VJNode targetNode, Port outputPort)
        {
            if (sourceNode is TriggerBoxNode && targetNode is TriggerBoxNode)
                return EdgeType.TriggerToTrigger;
            if (sourceNode is TriggerBoxNode && targetNode is ResultBoxNode)
                return EdgeType.TriggerToResult;
            if (sourceNode is ResultBoxNode rn && targetNode is DefaultStateNode && outputPort == rn.DefaultStatePort)
                return EdgeType.ResultToDefault;
            if (sourceNode is ResultBoxNode && targetNode is ResultBoxNode)
                return EdgeType.ResultToResult;

            return EdgeType.TriggerToResult;
        }

        private Port GetOutputPort(VJNode node, EdgeType edgeType)
        {
            if (node is ResultBoxNode resultNode && edgeType == EdgeType.ResultToDefault)
                return resultNode.DefaultStatePort;
            return node.BottomPort;
        }

        private Port GetInputPort(VJNode node)
        {
            return node.TopPort;
        }
    }
}
