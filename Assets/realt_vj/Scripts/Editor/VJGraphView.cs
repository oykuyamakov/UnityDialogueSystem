using System;
using System.Collections.Generic;
using System.Linq;
using RealtVJ.Data;
using UnityEditor;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealtVJ.Editor
{
    public class VJGraphView : GraphView
    {
        private readonly Vector2 m_DefaultNodeSize = new(260, 200);
        private VJNodeSearchWindow m_SearchWindow;
        private MiniMap m_MiniMap;
        private readonly EditorWindow m_EditorWindow;

        public VJGraphView(EditorWindow editorWindow)
        {
            m_EditorWindow = editorWindow;

            // Load stylesheet
            var stylesheet = Resources.Load<StyleSheet>("VJGraph");
            if (stylesheet != null)
                styleSheets.Add(stylesheet);

            SetupZoom(ContentZoomer.DefaultMinScale, 2f);

            this.AddManipulator(new ContentDragger());
            this.AddManipulator(new SelectionDragger());
            this.AddManipulator(new RectangleSelector());

            // Grid background
            var grid = new GridBackground();
            Insert(0, grid);
            grid.StretchToParentSize();

            InitializeMiniMap();
            AddSearchWindow(editorWindow);

            // Register keyboard shortcut for grouping
            RegisterCallback<KeyDownEvent>(OnKeyDown);
        }

        private void AddSearchWindow(EditorWindow editorWindow)
        {
            m_SearchWindow = ScriptableObject.CreateInstance<VJNodeSearchWindow>();
            m_SearchWindow.Init(this, editorWindow);

            // Right-click or space opens search
            nodeCreationRequest = context =>
                SearchWindow.Open(new SearchWindowContext(context.screenMousePosition), m_SearchWindow);
        }

        public void InitializeMiniMap()
        {
            if (m_MiniMap != null) return;
            m_MiniMap = new MiniMap { anchored = false };
            m_MiniMap.SetPosition(new Rect(10, 30, 200, 140));
            Add(m_MiniMap);
        }

        public void RemoveMiniMap()
        {
            if (m_MiniMap == null) return;
            Remove(m_MiniMap);
            m_MiniMap = null;
        }

        // -- Port Compatibility --

        public override List<Port> GetCompatiblePorts(Port startPort, NodeAdapter nodeAdapter)
        {
            var compatiblePorts = new List<Port>();

            ports.ForEach(port =>
            {
                if (startPort == port || startPort.node == port.node)
                    return;
                if (startPort.direction == port.direction)
                    return;

                var startNode = startPort.node as VJNode;
                var targetNode = port.node as VJNode;
                if (startNode == null || targetNode == null) return;

                if (IsValidConnection(startNode, startPort, targetNode, port))
                    compatiblePorts.Add(port);
            });

            return compatiblePorts;
        }

        private bool IsValidConnection(VJNode sourceNode, Port sourcePort, VJNode targetNode, Port targetPort)
        {
            VJNode outNode = sourcePort.direction == Direction.Output ? sourceNode : targetNode;
            VJNode inNode = sourcePort.direction == Direction.Input ? sourceNode : targetNode;
            Port outPort = sourcePort.direction == Direction.Output ? sourcePort : targetPort;

            // Trigger -> Trigger (AND chain)
            if (outNode is TriggerBoxNode && inNode is TriggerBoxNode)
                return true;

            // Trigger -> Result (rule creation)
            if (outNode is TriggerBoxNode && inNode is ResultBoxNode)
                return true;

            // Result -> Result (sequential chain) -- only via main output, not DefaultStatePort
            if (outNode is ResultBoxNode outResult && inNode is ResultBoxNode)
                return outPort != outResult.DefaultStatePort;

            // Result -> DefaultState (horizontal connection)
            if (outNode is ResultBoxNode resultNode && inNode is DefaultStateNode)
                return outPort == resultNode.DefaultStatePort;

            return false;
        }

        // -- Node Creation --

        public TriggerBoxNode CreateTriggerBoxNode(Vector2 position, TriggerBoxData data = null)
        {
            data ??= new TriggerBoxData(Guid.NewGuid().ToString(), position);
            var node = new TriggerBoxNode(data.Guid, data);
            node.SetPosition(new Rect(position, m_DefaultNodeSize));
            return node;
        }

        public ResultBoxNode CreateResultBoxNode(Vector2 position, ResultBoxData data = null)
        {
            data ??= new ResultBoxData(Guid.NewGuid().ToString(), position);
            var node = new ResultBoxNode(data.Guid, data);
            node.SetPosition(new Rect(position, m_DefaultNodeSize));
            return node;
        }

        public DefaultStateNode CreateDefaultStateNode(Vector2 position, DefaultStateBoxData data = null)
        {
            data ??= new DefaultStateBoxData(Guid.NewGuid().ToString(), position);
            var node = new DefaultStateNode(data.Guid, data);
            node.SetPosition(new Rect(position, new Vector2(220, 160)));
            return node;
        }

        public void AddCreateTriggerBoxNode(Vector2 position)
        {
            AddElement(CreateTriggerBoxNode(position));
        }

        public void AddCreateResultBoxNode(Vector2 position)
        {
            AddElement(CreateResultBoxNode(position));
        }

        public void AddCreateDefaultStateNode(Vector2 position)
        {
            AddElement(CreateDefaultStateNode(position));
        }

        // -- Grouping --

        public void GroupSelectedNodes(string groupName = null)
        {
            var selectedNodes = selection.OfType<VJNode>().ToList();
            if (selectedNodes.Count == 0) return;

            groupName ??= "Rule Group";

            var group = new Group { title = groupName };
            group.AddToClassList("group-node");
            AddElement(group);

            foreach (var node in selectedNodes)
                group.AddElement(node);
        }

        private void OnKeyDown(KeyDownEvent evt)
        {
            // Ctrl+G to group selected nodes
            if (evt.keyCode == KeyCode.G && evt.ctrlKey)
            {
                GroupSelectedNodes();
                evt.StopPropagation();
            }
        }

        // -- Helpers --

        public List<TriggerBoxNode> GetTriggerBoxNodes() => nodes.OfType<TriggerBoxNode>().ToList();
        public List<ResultBoxNode> GetResultBoxNodes() => nodes.OfType<ResultBoxNode>().ToList();
        public List<DefaultStateNode> GetDefaultStateNodes() => nodes.OfType<DefaultStateNode>().ToList();
    }

}
