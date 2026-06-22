using System.Collections.Generic;
using System.Linq;
using UnityDialogueSystem.Scripts.Actions;
using UnityDialogueSystem.Scripts.Data;
using UnityDialogueSystem.Scripts.Editor.CustomElements;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace UnityDialogueSystem.Scripts.Editor
{
    public class GraphSaveUtility
    {
        private DialogueGraphView m_TargetGraphView;
        private DialogueContainer m_DialogueContainerCache;

        private List<NpcNode> m_NpcNodes => m_TargetGraphView.nodes.OfType<NpcNode>().ToList();
        private List<PlayerNode> m_PlayerNodes => m_TargetGraphView.nodes.OfType<PlayerNode>().ToList();
        private List<StartNode> m_StartNodes => m_TargetGraphView.nodes.OfType<StartNode>().ToList();
        private NpcName m_OwnerNpc => m_TargetGraphView.OwnerNpc;

        public static GraphSaveUtility GetInstance(DialogueGraphView targetGraphView)
        {
            return new GraphSaveUtility
            {
                m_TargetGraphView = targetGraphView
            };
        }

        public void SaveGraph(string fileName, bool recovery = false)
        {
            DialogueContainer dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
            dialogueContainer.NpcName = m_OwnerNpc;

            foreach (StartNode startNode in m_StartNodes)
            {
                TryGetNodeSingleTargetGuid(startNode, out string targetGuid);

                StartNodeData newStartNodeData = new(
                    startNode.GUID,
                    startNode.GetPosition().position,
                    targetGuid,
                    startNode.Conditions);

                dialogueContainer.StartNodes.Add(newStartNodeData);
            }

            foreach (NpcNode npcNode in m_NpcNodes)
            {
                TryGetNodeSingleTargetGuid(npcNode, out string targetGuid);

                NpcNodeData newNpcNode = new(
                    npcNode.GUID,
                    npcNode.GetPosition().position,
                    targetGuid,
                    npcNode.GetLines(),
                    npcNode.MainOwner,
                    npcNode.ExecutionMode);

                dialogueContainer.NpcNodes.Add(newNpcNode);
            }

            foreach (PlayerNode plNode in m_PlayerNodes)
            {
                PlayerNodeData plNodeData = new(plNode.GUID, plNode.GetPosition().position, new List<PlayerLine>());
                List<PlayerLineBox> plLinesBoxes = plNode.GetLinesBoxes();

                foreach (PlayerLineBox plLine in plLinesBoxes)
                {
                    string targetGuid = string.Empty;
                    TryGetPortSingleTargetGuid(plNode, plLine.AnswerPort, out targetGuid);

                    PlayerLine sourceLine = (PlayerLine)plLine.GetLine();

                    plNodeData.PlayerLines.Add(new PlayerLine(
                        sourceLine.DialogueLine,
                        plNode.GUID,
                        targetGuid,
                        sourceLine.Conditions,
                        sourceLine.Triggers));
                }

                dialogueContainer.PlayerNodes.Add(plNodeData);
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources"))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            if (!AssetDatabase.IsValidFolder("Assets/Resources/Dialogues"))
            {
                AssetDatabase.CreateFolder("Assets/Resources", "Dialogues");
            }

            if (recovery)
            {
                AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/Dialogues/{fileName}.asset");
                AssetDatabase.SaveAssets();
            }
            else
            {
                if (!AssetDatabase.IsValidFolder($"Assets/Resources/Dialogues/{m_OwnerNpc}"))
                {
                    AssetDatabase.CreateFolder("Assets/Resources/Dialogues", $"{m_OwnerNpc}");
                }

                AssetDatabase.CreateAsset(dialogueContainer, $"Assets/Resources/Dialogues/{fileName}.asset");
                AssetDatabase.SaveAssets();
            }
        }

        private bool TryGetNodeSingleTargetGuid(DialogueNode node, out string targetGuid)
        {
            targetGuid = string.Empty;

            if (!node.TryGetOutputEdges(out IEnumerable<Edge> outEdges, m_TargetGraphView))
            {
                return false;
            }

            Edge[] edges = outEdges as Edge[] ?? outEdges.ToArray();

            if (edges.Length > 1)
            {
                Debug.LogWarning("Node has more than one output edge. Only first one will be used.");
            }

            if (edges.First().input.node is not DialogueNode targetNode)
            {
                return false;
            }

            targetGuid = targetNode.GUID;
            return true;
        }

        private bool TryGetPortSingleTargetGuid(PlayerNode node, Port port, out string targetGuid)
        {
            targetGuid = string.Empty;

            if (!node.TryGetAnswerEdges(out IEnumerable<Edge> outEdges, m_TargetGraphView, port))
            {
                return false;
            }

            Edge[] edges = outEdges as Edge[] ?? outEdges.ToArray();

            if (edges.Length > 1)
            {
                Debug.LogWarning("Player node port has more than one output edge. Only first one will be used.");
            }

            if (edges.First().input.node is not DialogueNode targetNode)
            {
                return false;
            }

            targetGuid = targetNode.GUID;
            return true;
        }

        public void LoadGraph(string fileName)
        {
            m_DialogueContainerCache = Resources.Load<DialogueContainer>($"Dialogues/{fileName}");

            if (m_DialogueContainerCache == null)
            {
                EditorUtility.DisplayDialog(
                    "File not Found dude",
                    "Target graph does not exist at resources/dialogues",
                    "I need to get my shit together");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
        }

        private void ClearGraph()
        {
            foreach (StartNode sNode in m_StartNodes)
            {
                if (sNode.TryGetOutputEdges(out IEnumerable<Edge> edges, m_TargetGraphView))
                {
                    foreach (Edge edge in edges)
                    {
                        m_TargetGraphView.RemoveElement(edge);
                    }
                }

                m_TargetGraphView.RemoveElement(sNode);
            }

            foreach (PlayerNode plNode in m_PlayerNodes)
            {
                foreach (Port port in plNode.AnswerPorts)
                {
                    List<Edge> edges = port.connections.ToList();
                    edges.ForEach(edge => m_TargetGraphView.RemoveElement(edge));
                }

                m_TargetGraphView.RemoveElement(plNode);
            }

            foreach (NpcNode npcNode in m_NpcNodes)
            {
                npcNode.TryGetOutputEdges(out IEnumerable<Edge> edges, m_TargetGraphView);

                foreach (Edge edge in edges)
                {
                    m_TargetGraphView.RemoveElement(edge);
                }

                m_TargetGraphView.RemoveElement(npcNode);
            }
        }

        private void CreateNodes()
        {
            foreach (StartNodeData startNodeData in m_DialogueContainerCache.StartNodes)
            {
                StartNode newStartNode = m_TargetGraphView.ReInstantiateStartNode(
                    startNodeData.Guid,
                    "Start",
                    startNodeData.Conditions,
                    startNodeData.GraphPosition);

                m_TargetGraphView.AddElement(newStartNode);
            }

            foreach (PlayerNodeData playerNodeData in m_DialogueContainerCache.PlayerNodes)
            {
                PlayerNode newNode = m_TargetGraphView.ReInstantiateAnswerNode(
                    playerNodeData.Guid,
                    "Player Line",
                    playerNodeData.PlayerLines,
                    playerNodeData.GraphPosition);

                m_TargetGraphView.AddElement(newNode);
            }

            foreach (NpcNodeData npcNodeData in m_DialogueContainerCache.NpcNodes)
            {
                NpcNode newNode = m_TargetGraphView.ReInstantiateNpcNode(
                    npcNodeData.Guid,
                    npcNodeData.Lines,
                    npcNodeData.GraphPosition,
                    npcNodeData.OwnerNpc,
                    npcNodeData.ExecutionMode);

                m_TargetGraphView.AddElement(newNode);
            }
        }

        private void ConnectNodes()
        {
            List<DialogueNode> allNodes = m_NpcNodes.Cast<DialogueNode>()
                .Concat(m_PlayerNodes.Cast<DialogueNode>())
                .ToList();

            foreach (StartNode startNode in m_StartNodes)
            {
                if (!m_DialogueContainerCache.TryGetStartNodeByGuid(startNode.GUID, out StartNodeData sNodeData))
                {
                    return;
                }

                if (sNodeData.TargetNodeGuid != string.Empty)
                {
                    DialogueNode linkNode = allNodes.First(x => x.GUID == sNodeData.TargetNodeGuid);
                    LinkNodes(startNode.OutputPort, linkNode.InputPort);
                }
            }

            foreach (PlayerNode plNode in m_PlayerNodes)
            {
                if (!m_DialogueContainerCache.TryGetPlayerNodeDataByGuid(plNode.GUID, out PlayerNodeData plData))
                {
                    return;
                }

                List<PlayerLine> lines = plData.PlayerLines;
                List<PlayerLineBox> linesBoxes = plNode.GetLinesBoxes();

                foreach (PlayerLineBox lineBox in linesBoxes)
                {
                    string targetNodeGuid = lines.First(x => x.TargetNodeGuid == lineBox.AnswerPort.portName)
                        .TargetNodeGuid;

                    if (targetNodeGuid == string.Empty)
                    {
                        continue;
                    }

                    LinkNodes(lineBox.AnswerPort, allNodes.First(x => x.GUID == targetNodeGuid).InputPort);
                }
            }

            foreach (NpcNode npcNode in m_NpcNodes)
            {
                if (!m_DialogueContainerCache.TryGetNpcNodeDataByGuid(npcNode.GUID, out NpcNodeData npcData))
                {
                    return;
                }

                if (npcData.TargetNodeGuid == string.Empty)
                {
                    continue;
                }

                LinkNodes(npcNode.OutputPort, allNodes.First(x => x.GUID == npcData.TargetNodeGuid).InputPort);
            }
        }

        private void LinkNodes(Port output, Port input)
        {
            Edge newEdge = new()
            {
                output = output,
                input = input
            };

            newEdge.input.Connect(newEdge);
            newEdge.output.Connect(newEdge);

            m_TargetGraphView.Add(newEdge);
        }
    }
}