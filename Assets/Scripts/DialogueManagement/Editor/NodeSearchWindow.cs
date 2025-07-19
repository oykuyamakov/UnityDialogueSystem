using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueManagement.Editor
{
    public class NodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private DialogueGraphView m_GraphView;
        private EditorWindow m_Window;
        private Texture2D m_PlayerIndentationIcon;
        private Texture2D m_NpcIndentationIcon;
        private Texture2D m_StartIndentationIcon;
        
        public void Init(DialogueGraphView graphView, EditorWindow window)
        {
            m_GraphView = graphView;
            m_Window = window;
            m_StartIndentationIcon = CreateIndentationIcon(new Color(0.31f, 1f, 0.27f));
            m_PlayerIndentationIcon = CreateIndentationIcon(new Color(0.29f, 0.93f, 1f));
            m_NpcIndentationIcon = CreateIndentationIcon(new Color(0.87f, 0.35f, 0.9f));
        }
        
        private Texture2D CreateIndentationIcon(Color color)
        {
            var indentationIcon = new Texture2D(1, 1);
            indentationIcon.SetPixel(0, 0, color);
            indentationIcon.Apply();
            return indentationIcon;
        }
        
        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            var tree = new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Dialogue Node"), level: 0),
                new SearchTreeEntry(new GUIContent("Start Node", m_StartIndentationIcon))
                {
                    userData = new SearchUserData(NodeType.Start), level = 1,
                },
                new SearchTreeEntry(new GUIContent("Npc Node", m_NpcIndentationIcon))
                {
                    userData = new SearchUserData(NodeType.Npc), level = 1,
                },
                new SearchTreeEntry(new GUIContent("Player Node", m_PlayerIndentationIcon))
                {
                    userData = new SearchUserData(NodeType.Player), level = 1,
                },
            };
            return tree;
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            if(searchTreeEntry.userData is not SearchUserData searchUserData) return false;
            
            var worldMousePosition = m_Window.rootVisualElement.ChangeCoordinatesTo(m_Window.rootVisualElement.parent,
                context.screenMousePosition - m_Window.position.position);
            var localMousePosition = m_GraphView.contentViewContainer.WorldToLocal(worldMousePosition);
            
            switch (searchTreeEntry.userData as SearchUserData)
            {
                case {NodeType: NodeType.Npc}:
                    m_GraphView.AddCreateNpcNode(localMousePosition);
                    return true;
                case {NodeType: NodeType.Player}:
                    m_GraphView.AddCreatePlayerNode(localMousePosition);
                    return true;
                case {NodeType: NodeType.Start}:
                    m_GraphView.AddCreateStarNode(localMousePosition);
                    return true;
                default:
                    return false;
            }
        }
    }

    public class SearchUserData
    {
        public NodeType NodeType { get; set; }
        
        public SearchUserData(NodeType nodeType)
        {
            NodeType = nodeType;
        }
    }

    public enum NodeType
    {
        Npc,
        Player,
        Start
    }
}
