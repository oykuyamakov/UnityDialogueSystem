using System;
using System.Collections.Generic;
using System.Linq;
using DialogueManagement.Actions;
using DialogueManagement.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueManagement.Editor
{
    [Serializable]
    public class DialogueGraphView : GraphView
    {
        private Vector2 DefaultNodeSize = new Vector2(200, 350);
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

            var grid = new GridBackground();
            Insert(0,grid);
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
                return;

            m_MiniMap = new MiniMap { anchored = false };
            m_MiniMap.SetPosition(new Rect(10, 30, 200, 140));
            Add(m_MiniMap);
        }
        
        private void AddMiniMap()
        {
            m_MiniMap = new MiniMap { anchored = true };
            m_MiniMap.SetPosition(new Rect(10, 30, 200, 140)); // position inside the graph
            Add(m_MiniMap);
        }
        
        public void RemoveMiniMap()
        {
            if (m_MiniMap != null)
            {
                Remove(m_MiniMap);
                m_MiniMap = null;
            }
        }

        
        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();
            ports.ForEach((port) =>
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
            AddElement(CreateAnswerNode(Guid.NewGuid().ToString(), "Player Node", position));
        }
        
        public PlayerNode ReInstantiateAnswerNode(string guid, string nodeTitle, List<PlayerLine> playerLines, Vector3 position)
        {
            var newPlNode = CreateAnswerNode(guid, nodeTitle, position);

            foreach (var answer in playerLines)
            {
                newPlNode.AddNewLineBox(answer);
                //newAnswerNode.AddAnswerPort(choice.Text, choice.GUID);
            }
            
            newPlNode.SetPosition(new Rect(position, DefaultNodeSize));
            return newPlNode;
        }
        
        private PlayerNode CreateAnswerNode(string guid, string nodeTitle, Vector2 position)
        {
            var answerNode = new PlayerNode(guid, nodeTitle);

            var rect = answerNode.GetPosition();
            rect.position = position;
            rect.size = DefaultNodeSize;
            answerNode.SetPosition(rect);
            
            m_Loaded = true;
            
            return answerNode;
        }
        
        public void AddCreateStarNode(Vector2 position)
        {
            AddElement(CreateStarNode(Guid.NewGuid().ToString(), "Start Node",new DialogueConditionContainer(), position));
        }

        private StartNode CreateStarNode(string guid, string nodeTitle,  DialogueConditionContainer lineReqs, Vector2 position)
        {
            var node = new StartNode(guid, nodeTitle, lineReqs);

            node.SetPosition(new Rect(position, DefaultNodeSize));
            node.RefreshExpandedState();
            node.RefreshPorts();
            
            m_Loaded = true;
            
            return node;
        }

        public StartNode ReInstantiateStartNode(string guid, string nodeTitle, DialogueConditionContainer lineReqs, Vector2 position)
        {
            return CreateStarNode(guid, nodeTitle, lineReqs, position);
        }
        
        public void AddCreateNpcNode(Vector2 position)
        {
            AddElement(CreateNpcNode(Guid.NewGuid().ToString(), new List<NpcLine>(),new List<DialogueTrigger>(), position, m_OwnerNpc));
        }

        public NpcNode ReInstantiateNpcNode(string guid, List<NpcLine> lines,List<DialogueTrigger> triggers, Vector2 position, NpcName ownerNpc)
        {
            return CreateNpcNode(guid, lines, triggers,position, ownerNpc);
        }

        private NpcNode CreateNpcNode(string guid,List<NpcLine> lines,List<DialogueTrigger> triggers, Vector2 position, NpcName owner)
        {
            var npcNode = new NpcNode(guid, m_OwnerNpc + "Npc Node", lines, triggers, owner);
            
            npcNode.SetPosition(new Rect(position, DefaultNodeSize));
            npcNode.RefreshExpandedState();
            npcNode.RefreshPorts();
            
            m_Loaded = true;

            return npcNode;
        }

        public void UpdateNodeOwner(NpcName mainOwner)
        {
            if(mainOwner == NpcName.Any)
                return;
            
            var npcNodes = nodes.OfType<NpcNode>().ToList();
            foreach (var npcNode in npcNodes)
            {
                npcNode.MainOwner = mainOwner;
            }
        }
    }
}
