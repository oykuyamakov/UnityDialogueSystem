using System;
using System.Collections.Generic;
using System.Linq;
using DialogueManagement.Actions;
using DialogueManagement.Data;
using DialogueManagement.Editor.CustomElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UIElements;

namespace DialogueManagement.Editor
{
    public class PlayerNode : DialogueNode
    {
        public List<Port> AnswerPorts => outputContainer.Query<Port>().ToList();
        
        private List<PlayerLineBox> m_LinesBoxes = new();
        
        public override void RemoveLineBox(LineBox lineBox)
        {
            if (lineBox is not PlayerLineBox plLineBox)
            {
                throw new InvalidCastException("LineBox is not of type PlayerLineBox");
            }
            
            var index = m_LinesBoxes.IndexOf(plLineBox);
            if (index < 0) 
                return;

            plLineBox.AnswerPort.DisconnectAll();
            
            NodeContentContainer.Remove(plLineBox);
            m_LinesBoxes.RemoveAt(index);
            
            RefreshPorts();
            RefreshExpandedState();
        }

        public List<PlayerLine> GetLines()
        {
            return m_LinesBoxes.Select(lineBox => lineBox.GetLine() as PlayerLine).ToList();
        }
        
        public List<PlayerLineBox> GetLinesBoxes()
        {
            return m_LinesBoxes;
        }
        
        public bool TryGetAnswerEdges(out IEnumerable<Edge> edges, GraphView graphView, Port answer)
        {
            edges = graphView.edges.Where(e => e.output == answer);
            return edges.Any();
        }
        
        public PlayerNode(string guid, string nodeTitle) : base(guid, nodeTitle)
        {
            GeneratePort(Direction.Input, "Prev");
            
            NodeContentContainer = new VisualElement();
            NodeContentContainer.style.flexDirection = FlexDirection.Column;
            
            var button = new Button(() =>
            {
                AddNewLineBox(new PlayerLine("...", this.GUID, Guid.NewGuid().ToString(), new DialogueConditionContainer()));
            });
            button.text = "+ Line";
            
            titleContainer.Add(button);

            RefreshExpandedState();
            RefreshPorts();
            
            outputContainer.Add(NodeContentContainer);
        }

        public void AddNewLineBox(PlayerLine line)
        {
            var newBox = new PlayerLineBox(line, this);
            m_LinesBoxes.Add(newBox);
        }
    }
}
