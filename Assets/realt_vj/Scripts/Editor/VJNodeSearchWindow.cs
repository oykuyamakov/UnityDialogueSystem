using System.Collections.Generic;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealtVJ.Editor
{
    public class VJNodeSearchWindow : ScriptableObject, ISearchWindowProvider
    {
        private VJGraphView m_GraphView;
        private EditorWindow m_Window;
        private Texture2D m_TriggerIcon;
        private Texture2D m_ResultIcon;
        private Texture2D m_DefaultIcon;

        public void Init(VJGraphView graphView, EditorWindow window)
        {
            m_GraphView = graphView;
            m_Window = window;
            m_TriggerIcon = CreateIcon(new Color(0f, 0.83f, 1f));     // Electric blue
            m_ResultIcon = CreateIcon(new Color(0.75f, 0.33f, 0.98f)); // Rich purple
            m_DefaultIcon = CreateIcon(new Color(0.49f, 0.23f, 0.93f)); // Lavender
        }

        private Texture2D CreateIcon(Color color)
        {
            var icon = new Texture2D(1, 1);
            icon.SetPixel(0, 0, color);
            icon.Apply();
            return icon;
        }

        public List<SearchTreeEntry> CreateSearchTree(SearchWindowContext context)
        {
            return new List<SearchTreeEntry>
            {
                new SearchTreeGroupEntry(new GUIContent("Create Node"), 0),
                new SearchTreeGroupEntry(new GUIContent("Boxes"), 1),
                new SearchTreeEntry(new GUIContent("Trigger Box", m_TriggerIcon))
                {
                    userData = VJNodeType.TriggerBox, level = 2
                },
                new SearchTreeEntry(new GUIContent("Result Box", m_ResultIcon))
                {
                    userData = VJNodeType.ResultBox, level = 2
                },
                new SearchTreeEntry(new GUIContent("Default State", m_DefaultIcon))
                {
                    userData = VJNodeType.DefaultState, level = 2
                }
            };
        }

        public bool OnSelectEntry(SearchTreeEntry searchTreeEntry, SearchWindowContext context)
        {
            var worldMousePosition = m_Window.rootVisualElement.ChangeCoordinatesTo(
                m_Window.rootVisualElement.parent,
                context.screenMousePosition - m_Window.position.position);
            var localMousePosition = m_GraphView.contentViewContainer.WorldToLocal(worldMousePosition);

            switch ((VJNodeType)searchTreeEntry.userData)
            {
                case VJNodeType.TriggerBox:
                    m_GraphView.AddCreateTriggerBoxNode(localMousePosition);
                    return true;
                case VJNodeType.ResultBox:
                    m_GraphView.AddCreateResultBoxNode(localMousePosition);
                    return true;
                case VJNodeType.DefaultState:
                    m_GraphView.AddCreateDefaultStateNode(localMousePosition);
                    return true;
                default:
                    return false;
            }
        }
    }

    public enum VJNodeType
    {
        TriggerBox,
        ResultBox,
        DefaultState
    }
}
