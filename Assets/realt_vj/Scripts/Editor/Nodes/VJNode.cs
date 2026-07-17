using System.Collections.Generic;
using System.Linq;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealtVJ.Editor
{
    public abstract class VJNode : Node
    {
        public string GUID { get; }

        public Port TopPort { get; protected set; }
        public Port BottomPort { get; protected set; }

        protected VisualElement ContentBox;

        // Custom port containers for vertical layout
        protected VisualElement TopPortContainer { get; private set; }
        protected VisualElement BottomPortContainer { get; private set; }

        protected VJNode(string guid, string nodeTitle)
        {
            GUID = guid;
            title = nodeTitle;
            AddToClassList("vj-node");

            SetupVerticalLayout();
        }

        private void SetupVerticalLayout()
        {
            // Hide the default #top container (holds input|divider|output horizontally)
            var topContainer = this.Q("top");
            if (topContainer != null)
                topContainer.style.display = DisplayStyle.None;

            // Create top port container — inserted before the title
            TopPortContainer = new VisualElement();
            TopPortContainer.AddToClassList("vj-port-top");
            TopPortContainer.style.flexDirection = FlexDirection.Row;
            TopPortContainer.style.justifyContent = Justify.Center;
            TopPortContainer.style.alignItems = Align.Center;
            TopPortContainer.style.paddingTop = 4;
            TopPortContainer.style.paddingBottom = 2;

            var nodeBorder = this.Q("node-border");
            if (nodeBorder != null)
                nodeBorder.Insert(0, TopPortContainer);

            // Create bottom port container — appended after everything
            BottomPortContainer = new VisualElement();
            BottomPortContainer.AddToClassList("vj-port-bottom");
            BottomPortContainer.style.flexDirection = FlexDirection.Row;
            BottomPortContainer.style.justifyContent = Justify.Center;
            BottomPortContainer.style.alignItems = Align.Center;
            BottomPortContainer.style.paddingTop = 2;
            BottomPortContainer.style.paddingBottom = 4;

            if (nodeBorder != null)
                nodeBorder.Add(BottomPortContainer);
        }

        protected Port AddInputPort(string portName, Port.Capacity capacity = Port.Capacity.Multi)
        {
            var port = InstantiatePort(Orientation.Vertical, Direction.Input, capacity, typeof(float));
            port.portName = portName;
            TopPortContainer.Add(port);
            return port;
        }

        protected Port AddOutputPort(string portName, Port.Capacity capacity = Port.Capacity.Multi)
        {
            var port = InstantiatePort(Orientation.Vertical, Direction.Output, capacity, typeof(float));
            port.portName = portName;
            BottomPortContainer.Add(port);
            return port;
        }

        /// <summary>
        /// Add a port to the right side of the node (horizontal output, for DefaultState connections).
        /// </summary>
        protected Port AddSideOutputPort(string portName, Port.Capacity capacity = Port.Capacity.Single)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Output, capacity, typeof(float));
            port.portName = portName;
            outputContainer.Add(port);
            ShowSidePortContainer();
            return port;
        }

        /// <summary>
        /// Add a port to the left side of the node (horizontal input, for DefaultState connections).
        /// </summary>
        protected Port AddSideInputPort(string portName, Port.Capacity capacity = Port.Capacity.Multi)
        {
            var port = InstantiatePort(Orientation.Horizontal, Direction.Input, capacity, typeof(float));
            port.portName = portName;
            inputContainer.Add(port);
            ShowSidePortContainer();
            return port;
        }

        private void ShowSidePortContainer()
        {
            // Make the #top container visible for side ports
            var topContainer = this.Q("top");
            if (topContainer != null)
            {
                topContainer.style.display = DisplayStyle.Flex;
                // Hide divider between input and output containers
                var divider = this.Q("divider");
                if (divider != null)
                    divider.style.display = DisplayStyle.None;
            }
            RefreshPorts();
            RefreshExpandedState();
        }

        protected void SetupContentBox()
        {
            ContentBox = new VisualElement();
            ContentBox.AddToClassList("content-box");
            ContentBox.style.flexDirection = FlexDirection.Column;
            extensionContainer.Add(ContentBox);
            RefreshExpandedState();
        }

        public IEnumerable<Port> GetAllPorts()
        {
            var allPorts = new List<Port>();
            allPorts.AddRange(TopPortContainer.Children().OfType<Port>());
            allPorts.AddRange(BottomPortContainer.Children().OfType<Port>());
            allPorts.AddRange(inputContainer.Children().OfType<Port>());   // Side input ports
            allPorts.AddRange(outputContainer.Children().OfType<Port>()); // Side output ports
            return allPorts;
        }
    }

    internal static class VisualElementBorderExtensions
    {
        public static void SetBorder(this VisualElement element, Color color, float width)
        {
            element.style.borderBottomColor = color;
            element.style.borderTopColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderBottomWidth = width;
            element.style.borderTopWidth = width;
            element.style.borderLeftWidth = width;
            element.style.borderRightWidth = width;
        }
    }
}
