using System.Collections.Generic;
using System.Linq;
using DialogueManagement.Actions;
using DialogueManagement.Data;
//using Sirenix.Utilities;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

namespace DialogueManagement.Editor
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
            return new GraphSaveUtility()
            {
                m_TargetGraphView = targetGraphView
            };
        }

        public void SaveGraph(string fileName, bool recovery = false)
        {
            var dialogueContainer = ScriptableObject.CreateInstance<DialogueContainer>();
            
            dialogueContainer.NpcName = m_OwnerNpc;
            
            foreach (var startNode in m_StartNodes)
            {
                TryGetNodeSingleTargetGuid(startNode, out var targetGuid);
                
                var newStartNodeData =
                    new StartNodeData(startNode.GUID, startNode.GetPosition().position, targetGuid,
                        startNode.DialogueConditionContainer);
                
                dialogueContainer.StartNodes.Add(newStartNodeData);
            }

            foreach (var npcNode in m_NpcNodes)
            {
                TryGetNodeSingleTargetGuid(npcNode, out var targetGuid);
                
                var newNpcNode = new NpcNodeData
                    (npcNode.GUID, npcNode.GetPosition().position,targetGuid, npcNode.GetLines(),npcNode.GetTriggers(),npcNode.MainOwner);
                
                dialogueContainer.NpcNodes.Add(newNpcNode);
            }
            
            foreach(var plNode in m_PlayerNodes)
            {
                var plNodeData = new PlayerNodeData(plNode.GUID, plNode.GetPosition().position, new List<PlayerLine>());
                var plLinesBoxes = plNode.GetLinesBoxes();

                foreach (var plLine in plLinesBoxes)
                {
                    if (TryGetPortSingleTargetGuid(plNode, plLine.AnswerPort, out var targetGuid))
                    {
                        plNodeData.PlayerLines.Add(new PlayerLine(plLine.GetLine().DialogueLine, plNode.GUID, targetGuid, plLine.GetLine().DialogueConditionContainer));
                    }
                    else
                    {
                        Debug.Log("no out in here hkhkh");
                        plNodeData.PlayerLines.Add(new PlayerLine(plLine.GetLine().DialogueLine, plNode.GUID, string.Empty, plLine.GetLine().DialogueConditionContainer));
                    }
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
            if (!node.TryGetOutputEdges(out var outEdges, m_TargetGraphView)) 
                return false;
            
            var enumerable = outEdges as Edge[] ?? outEdges.ToArray();
            if(enumerable.Count() > 1)
                Debug.LogWarning("Start node has more than one output edge. This is not allowed. Only getting first one");

            if (enumerable.First().input.node is not DialogueNode targetNode) 
                return false;
                
            targetGuid = targetNode.GUID;
            return true;

        }

        private bool TryGetPortSingleTargetGuid(PlayerNode node, Port port, out string targetGuid)
        {
            targetGuid = string.Empty;
            if (!node.TryGetAnswerEdges(out var outEdges, m_TargetGraphView, port)) 
                return false;
            
            var enumerable = outEdges as Edge[] ?? outEdges.ToArray();
            if(enumerable.Count() > 1)
                Debug.LogWarning("pl node has more than one output edge. This is not allowed. Only getting first one");

            if (enumerable.First().input.node is not DialogueNode targetNode) 
                return false;
                
            targetGuid = targetNode.GUID;
            return true;

        }

        public void LoadGraph(string fileName)
        {
            m_DialogueContainerCache = Resources.Load<DialogueContainer>($"Dialogues/{fileName}");

            if (m_DialogueContainerCache == null)
            {
                EditorUtility.DisplayDialog("File not Found dude", "Target graph does not exist at resources/dialogues",
                    "I need to get my shit together");
                return;
            }

            ClearGraph();
            CreateNodes();
            ConnectNodes();
        }

        private void ClearGraph()
        {
            foreach (var sNode in m_StartNodes)
            {
                if (sNode.TryGetOutputEdges(out var edges, m_TargetGraphView))
                {
                    foreach (var edge in edges)
                    {
                        m_TargetGraphView.RemoveElement(edge);
                    }
                }
                m_TargetGraphView.RemoveElement(sNode);
            }
            

            foreach (var plNode in m_PlayerNodes)
            {
                foreach (var port in plNode.AnswerPorts)
                {
                    var edges = port.connections.ToList();
                    edges.ForEach(edge => m_TargetGraphView.RemoveElement(edge));
                }
                m_TargetGraphView.RemoveElement(plNode);
            }
            
            foreach (var npcNode in m_NpcNodes)
            {
                npcNode.TryGetOutputEdges(out var edges, m_TargetGraphView);
                foreach (var edge in edges)
                {
                    m_TargetGraphView.RemoveElement(edge);
                }
                
                m_TargetGraphView.RemoveElement(npcNode);
            }
        }

        private void CreateNodes()
        {
            foreach (var sLine in m_DialogueContainerCache.StartNodes)
            {
                var newStartNode = m_TargetGraphView.ReInstantiateStartNode(sLine.Guid, "Start", sLine.DialogueConditionContainer, sLine.GraphPosition);
                m_TargetGraphView.AddElement(newStartNode);
            }
            
            foreach (var plLine in m_DialogueContainerCache.PlayerNodes)
            {
                var newNode = m_TargetGraphView.ReInstantiateAnswerNode(plLine.Guid, "Player Line", plLine.PlayerLines, plLine.GraphPosition);
                m_TargetGraphView.AddElement(newNode);
            }

            foreach (var npcLine in m_DialogueContainerCache.NpcNodes)
            {
                var newNode = m_TargetGraphView.ReInstantiateNpcNode(npcLine.Guid, npcLine.m_Lines,npcLine.Triggers, npcLine.GraphPosition, npcLine.OwnerNpc);
                m_TargetGraphView.AddElement(newNode);
            }
        }

        private void ConnectNodes()
        {
            var allNodes = m_NpcNodes.Concat(m_PlayerNodes.Cast<DialogueNode>()).ToList();
          
            foreach (var startNode in m_StartNodes)
            {
                if (!m_DialogueContainerCache.TryGetStartNodeByGuid(startNode.GUID, out var sNodeData))
                {
                    return;
                }

                if (sNodeData.TargetNodeGuid != string.Empty)
                {
                    var linkNode = allNodes.First(x => x.GUID == sNodeData.TargetNodeGuid);
                    LinkNodes(startNode.OutputPort, linkNode.InputPort);
                }
                
            }

            foreach (var plNode in m_PlayerNodes)
            {
                if (!m_DialogueContainerCache.TryGetPlayerNodeDataByGuid(plNode.GUID, out var pldata))
                {
                    return;
                }

                var lines = pldata.PlayerLines;
                
                var linesBoxes = plNode.GetLinesBoxes();

                foreach (var lineBox in linesBoxes)
                {
                    var targetNodeGuid = lines.First(x => x.TargetNodeGuid == lineBox.AnswerPort.portName).TargetNodeGuid;
                    if(targetNodeGuid  == string.Empty)
                    {
                        continue;
                    }
                    LinkNodes(lineBox.AnswerPort, allNodes.First(x => x.GUID == targetNodeGuid).InputPort);
                }
                
            }

            foreach (var npcNode in m_NpcNodes)
            {
                if (!m_DialogueContainerCache.TryGetNpcNodeDataByGuid(npcNode.GUID, out var npcData))
                {
                    return;
                }
                
                if(npcData.TargetNodeGuid == string.Empty)
                    continue;
                
                LinkNodes(npcNode.OutputPort, allNodes.First(x => x.GUID ==  npcData.TargetNodeGuid).InputPort);
            }
        }
        

        private void LinkNodes(Port output, Port input)
        {
            var newEdge = new Edge()
            {
                output = output,
                input = input
            };
            
            newEdge?.input.Connect(newEdge);
            newEdge?.output.Connect(newEdge);
            
            m_TargetGraphView.Add(newEdge);
        }
    }
}
