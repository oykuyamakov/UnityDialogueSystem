using System;
using System.Collections.Generic;
using System.Linq;
using UnityDialogueSystem.Scripts.Actions;
using UnityDialogueSystem.Scripts.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityDialogueSystem.Scripts.Editor
{
    [Serializable]
    public class DialogueGraphView : GraphView
    {
        private readonly Vector2 m_DefaultNodeSize = new(200, 350);
        public bool m_Loaded = false;

        public NpcName OwnerNpc
        {
            get => m_OwnerNpc;
            set
            {
                m_OwnerNpc = value;
                UpdateNodeOwner(m_OwnerNpc);
            }
        }

        private NpcName m_OwnerNpc;
        private NodeSearchWindow m_SearchWindow;
        private MiniMap m_MiniMap;

        public DialogueGraphView(EditorWindow editorWindow)
        {
            styleSheets.Add(Resources.Load<StyleSheet>("DialogueGraph"));
            styleSheets.Add(Resources.Load<StyleSheet>("Node"));
            SetupZoom(ContentZoomer.DefaultMinScale, 1.8f);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            GridBackground grid = new();
            Insert(0, grid);
            grid.StretchToParentSize();

            InitializeMiniMap();

            m_Loaded = false;
            AddSearchWindow(editorWindow);
        }

        private void AddSearchWindow(EditorWindow editorWindow)
        {
            m_SearchWindow = ScriptableObject.CreateInstance<NodeSearchWindow>();
            m_SearchWindow.Init(this, editorWindow);

            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchWindow);
        }

        public void InitializeMiniMap()
        {
            if (m_MiniMap != null)
            {
                return;
            }

            m_MiniMap = new MiniMap { anchored = false };
            m_MiniMap.SetPosition(new Rect(10, 30, 200, 140));
            Add(m_MiniMap);
        }

        public void RemoveMiniMap()
        {
            if (m_MiniMap == null)
            {
                return;
            }

            Remove(m_MiniMap);
            m_MiniMap = null;
        }

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            List<Port> compatiblePorts = new();

            ports.ForEach(port =>
            {
                if (startPort != port && startPort.node != port.node)
                {
                    compatiblePorts.Add(port);
                }
            });

            return compatiblePorts;
        }

        public void AddCreatePlayerNode(Vector2 position)
        {
            AddElement(CreateAnswerNode(Guid.NewGuid().ToString(), "Player", position));
        }

        public PlayerNode ReInstantiateAnswerNode(string guid, string nodeTitle, List<PlayerLine> playerLines, Vector3 position)
        {
            PlayerNode newPlayerNode = CreateAnswerNode(guid, nodeTitle, position);

            foreach (PlayerLine line in playerLines)
            {
                newPlayerNode.AddNewLineBox(line);
            }

            newPlayerNode.SetPosition(new Rect(position, m_DefaultNodeSize));
            return newPlayerNode;
        }

        private PlayerNode CreateAnswerNode(string guid, string nodeTitle, Vector2 position)
        {
            PlayerNode answerNode = new(guid, nodeTitle);

            Rect rect = answerNode.GetPosition();
            rect.position = position;
            rect.size = m_DefaultNodeSize;
            answerNode.SetPosition(rect);

            m_Loaded = true;
            return answerNode;
        }

        public void AddCreateStarNode(Vector2 position)
        {
            AddElement(CreateStarNode(Guid.NewGuid().ToString(), "Start", new ConditionSet(), position));
        }

        private StartNode CreateStarNode(string guid, string nodeTitle, ConditionSet conditions, Vector2 position)
        {
            StartNode node = new(guid, nodeTitle, conditions);

            node.SetPosition(new Rect(position, m_DefaultNodeSize));
            node.RefreshExpandedState();
            node.RefreshPorts();

            m_Loaded = true;
            return node;
        }

        public StartNode ReInstantiateStartNode(string guid, string nodeTitle, ConditionSet conditions, Vector2 position)
        {
            return CreateStarNode(guid, nodeTitle, conditions, position);
        }

        public void AddCreateNpcNode(Vector2 position)
        {
            AddElement(CreateNpcNode(Guid.NewGuid().ToString(), new List<NpcLine>(), position, m_OwnerNpc, NpcNodeExecutionMode.Single));
        }

        public NpcNode ReInstantiateNpcNode(string guid, List<NpcLine> lines, Vector2 position, NpcName ownerNpc, NpcNodeExecutionMode executionMode)
        {
            return CreateNpcNode(guid, lines, position, ownerNpc, executionMode);
        }

        private NpcNode CreateNpcNode(string guid, List<NpcLine> lines, Vector2 position, NpcName owner, NpcNodeExecutionMode executionMode)
        {
            NpcNode npcNode = new(guid, "Npc : ", lines, owner, executionMode);

            npcNode.SetPosition(new Rect(position, m_DefaultNodeSize));
            npcNode.RefreshExpandedState();
            npcNode.RefreshPorts();

            m_Loaded = true;
            return npcNode;
        }

        public void UpdateNodeOwner(NpcName mainOwner)
        {
            if (mainOwner == NpcName.Any)
            {
                return;
            }

            List<NpcNode> npcNodes = nodes.OfType<NpcNode>().ToList();

            foreach (NpcNode npcNode in npcNodes)
            {
                npcNode.MainOwner = mainOwner;
            }
        }
    }
}