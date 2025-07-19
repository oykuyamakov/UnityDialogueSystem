using System;
using System.Collections.Generic;
using System.Linq;
using DialogueManagement.Actions;
using DialogueManagement.Editor.CustomElements;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueManagement.Editor
{
    public abstract class DialogueNode : Node
    {
        public string GUID => m_GUID;
        private string m_GUID;
        
        public Port InputPort => inputContainer.Q<Port>();
        public Port OutputPort => outputContainer.Q<Port>();
        
        public VisualElement NodeContentContainer;

        protected DialogueNode(string guid, string nodeTitle)
        {
            m_GUID = guid;
            title = nodeTitle;
        }
        
        public abstract void RemoveLineBox(LineBox lineBox);
        
        public bool TryGetOutputEdges(out IEnumerable<Edge> edges,GraphView graphView)
        {
            edges = graphView.edges.Where(e => e.output == OutputPort);
            return edges.Any();
        }

        public Port GeneratePort(Direction portDirection, string portName,
            Port.Capacity capacity = Port.Capacity.Multi)
        {
            var port = this.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
            port.portName = portName;
            port.portType = typeof(float);
            
            var container = portDirection == Direction.Input ? inputContainer : outputContainer;
            container.Add(port);
            
            RefreshPorts();
            RefreshExpandedState();
            
            return port;
        }
    }

    public static class GraphNodeExtensions
    {
        public static VisualElement GetBorderBox(FlexDirection direction, Color color, bool bgColored = false)
        {
            var borderEl = new VisualElement();
            borderEl.style.flexDirection = direction;
            if(bgColored)
                borderEl.style.backgroundColor = color;
            borderEl.style.paddingTop = 4;
            borderEl.style.paddingBottom = 4;
            borderEl.style.paddingRight = 5;
            borderEl.style.paddingLeft = 5;
            
            borderEl.style.borderBottomColor = color;
            borderEl.style.borderBottomWidth = 2f;
            borderEl.style.paddingBottom = 1;
            
            borderEl.style.borderTopColor = color;
            borderEl.style.borderTopWidth = 2;
            borderEl.style.paddingTop = 1;

            borderEl.style.borderLeftColor = color;
            borderEl.style.borderLeftWidth = 2;
            borderEl.style.paddingLeft = 1;

            borderEl.style.borderRightColor = color;
            borderEl.style.borderRightWidth = 2f;
            borderEl.style.paddingRight = 1;
            
            return borderEl;
        }

        private static VisualElement GetItemActionContainer(ItemAction itemData)
        {
            var content = new VisualElement();
            content.style.flexDirection = FlexDirection.Row;
            
            var nameField = new EnumField(itemData.ItemName);
            nameField.RegisterValueChangedCallback(evt =>
            {
                itemData.ItemName = (ItemName)evt.newValue;
            });
            content.Add(nameField);
            
            var actionField = new EnumField(itemData.ItemActionType);
            actionField.RegisterValueChangedCallback(evt =>
            {
                itemData.ItemActionType = (ItemActionType)evt.newValue;
            });
            content.Add(actionField);

            return content;
        }

        private static VisualElement GetPlayerActionContainer(PlayerAction playerData)
        {
            var content = new VisualElement();
            
            var nameField = new EnumField(playerData.PlayerActionType);
            nameField.RegisterValueChangedCallback(evt =>
            {
                playerData.PlayerActionType = (PlayerActionType)evt.newValue;
            });
            content.Add(nameField);

            return content;
        }

        private static VisualElement GetLocationActionContainer(LocationAction locData)
        {
            var content = new VisualElement();
            
            var nameField = new EnumField(locData.LocationActionType);
            nameField.RegisterValueChangedCallback(evt =>
            {
                locData.LocationActionType = (LocationActionType)evt.newValue;
            });
            content.Add(nameField);

            return content;
        }

        private static VisualElement GetMusicActionContainer(MusicAction musicAction)
        {
            var content = new VisualElement();
            
            var nameField = new EnumField(musicAction.MusicActionType);
            nameField.RegisterValueChangedCallback(evt =>
            {
                musicAction.MusicActionType = (MusicActionType)evt.newValue;
            });
            content.Add(nameField);

            return content;
        }

        private static VisualElement GetGoActionContainer(GameObjectAction gameObjectAction)
        {
            var content = new VisualElement();
            
            var nameField = new EnumField(gameObjectAction.GOActionType);
            nameField.RegisterValueChangedCallback(evt =>
            {
                gameObjectAction.GOActionType = (GOActionType)evt.newValue;
            });
            content.Add(nameField);

            return content;
        }
        
        private static void AddActionTypeContainer(this VisualElement container, ActionRef data)
        {
            switch (data.Type)
            {
                case ActionType.Item:
                    container.Add(GetItemActionContainer(data as ItemAction));
                    break;
                case ActionType.Player:
                    container.Add(GetPlayerActionContainer(data as PlayerAction));
                    break;
                case ActionType.Location:
                    container.Add(GetLocationActionContainer(data as LocationAction));
                    break;
                case ActionType.Music:
                    container.Add(GetMusicActionContainer(data as MusicAction));
                    break;
                case ActionType.GameObject:
                    container.Add(GetGoActionContainer(data as GameObjectAction));
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        
        private static ActionRef QuickGetType(ActionType type)
        {
            return type switch
            {
                ActionType.Item => new ItemAction(),
                ActionType.Player => new PlayerAction(),
                ActionType.Location => new LocationAction(),
                ActionType.Music => new MusicAction(),
                ActionType.GameObject => new GameObjectAction(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        private static void AddDetailsBox(this VisualElement detailBox,DialogueConditionContainer dialogueCondition)
        {
            var labelLoc = new Label(" AT : ");
            detailBox.Add(labelLoc);
            
            var locationField = new EnumField(dialogueCondition.Location);
            locationField.RegisterValueChangedCallback(evt =>
            {
                dialogueCondition.Location = (Location)evt.newValue;
            });
            locationField.style.flexBasis =  new StyleLength( 50);
            locationField.style.flexGrow = 150f;
            locationField.style.flexShrink = 30f;
            detailBox.Add(locationField);
            
            var labelPersist = new Label(" permanent ");
                
            var togglePersist = new Toggle();
            togglePersist.value = dialogueCondition.Persistent;
            togglePersist.RegisterValueChangedCallback(evt => { dialogueCondition.Persistent = evt.newValue; });
            togglePersist.style.display = DisplayStyle.Flex;
                
            detailBox.Add(labelPersist);
            detailBox.Add(togglePersist);
            
            var labelAsap = new Label(" cut ");

            var toggleAsap = new Toggle();
            toggleAsap.value = dialogueCondition.Cut;
            toggleAsap.RegisterValueChangedCallback(evt => { dialogueCondition.Cut = evt.newValue; });
            
            detailBox.Add(labelAsap);
            detailBox.Add(toggleAsap);
        }

        public static VisualElement GetConditionBox(DialogueConditionContainer dialogueConditions, bool hasDetails)
        { 
            var allBox = new VisualElement();
            allBox.style.flexDirection = FlexDirection.Column;
            allBox.style.alignSelf = Align.Center;
            
            var headerBox = new VisualElement();
            headerBox.style.flexDirection = FlexDirection.Row;
            headerBox.style.alignItems = Align.Center;
            headerBox.style.alignSelf = Align.Center;
            headerBox.style.marginTop = 10f;
            headerBox.style.marginBottom = 10f;
            
            var detailBox = new VisualElement();
            detailBox.style.flexDirection = FlexDirection.Row;
            detailBox.style.alignItems = Align.Center;
            detailBox.style.alignContent = Align.Center;
            detailBox.style.marginBottom = 20f;
            
            var actionsBox = new VisualElement();
            actionsBox.style.flexDirection = FlexDirection.Column;
            
            var label = new Label(" -   CONDITIONS   - ");
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.marginLeft = 20;
            label.style.marginRight = 5;
            label.style.alignContent = Align.Center;
            label.style.color = new Color(0.98f, 0.46f, 0.46f);
            headerBox.Add(label);

            actionsBox.style.display = dialogueConditions.DebugIsInitialized() ? DisplayStyle.Flex : DisplayStyle.None;
            
            foreach (var act in dialogueConditions.Get())
            {
                actionsBox.AddNewActionBox(dialogueConditions, act);
            }
            
            var addButton = new Button(() => {
                actionsBox.AddNewActionBox(dialogueConditions, dialogueConditions.Add(new ItemAction()));
                actionsBox.style.display = DisplayStyle.Flex;
                detailBox.style.display =  DisplayStyle.Flex;
            });
            addButton.text = "  + ";
            addButton.style.alignItems = Align.FlexEnd;
            addButton.style.color = new Color(0.98f, 0.46f, 0.46f);
            headerBox.Add(addButton);
            
            if (hasDetails)
            {
                detailBox.AddDetailsBox(dialogueConditions);
            }
            
            allBox.Add(headerBox);
            allBox.Add(detailBox);
            allBox.Add(actionsBox);
            
            var borderColor = new Color(0.62f, 0.31f, 0.31f, 0.42f);
            allBox.DrawFrameAround(borderColor);

            return allBox;
        }

        private static void AddNewActionBox(this VisualElement container, DialogueConditionContainer lineReqs, DialogueCondition dialogueCondition)
        {
            if (lineReqs.Get() == null || lineReqs.ConditionCount == 0 ||  dialogueCondition == null)
            {
                //Debug.LogError($"Index {index} is out of bounds for Actions list with count {requirementContainer.Actions.Count}");
                return;
            }

            var index = lineReqs.Get().IndexOf(dialogueCondition) + 1;
                
            var reqBox = new VisualElement();
            reqBox.style.flexDirection = FlexDirection.Row;
            
            var label = new Label($"  #:{index} ");
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize = 10;
            label.style.color = new Color(0.98f, 0.46f, 0.46f);
            reqBox.Add(label);
                
            reqBox.Add(GetActionBox(dialogueCondition));
                
            var deleteReq = new Button(() => {
            {
                lineReqs.Remove(dialogueCondition);
                container.Remove(reqBox);
                
                container.style.display = lineReqs.DebugIsInitialized() ? DisplayStyle.Flex : DisplayStyle.None;
            } });
                
            deleteReq.text = "x";
            deleteReq.style.alignItems = Align.Center;
            deleteReq.style.color = new Color(0.98f, 0.46f, 0.46f);

            reqBox.Add(deleteReq);
            
            var borderColor = new Color(0.62f, 0.31f, 0.31f, 0.42f);
            reqBox.DrawFrameAround(borderColor);
            
            reqBox.style.marginLeft = 10f;
            reqBox.style.marginRight = 10f;
            reqBox.style.marginTop = 4f;
            reqBox.style.marginBottom = 4f;
                
            container.Add(reqBox);
        }
        
        public static void DrawFrameAround(this VisualElement element, Color color)
        {
            element.style.borderBottomColor = color;
            element.style.borderTopColor = color;
            element.style.borderLeftColor = color;
            element.style.borderRightColor = color;
            element.style.borderBottomWidth = 2f;
            element.style.borderTopWidth = 2f;
            element.style.borderLeftWidth = 2f;
            element.style.borderRightWidth = 2f;
        }
        
        public static VisualElement GetActionBox(DialogueAction dialogueAction)
        {
            var reqContent = new VisualElement();
            reqContent.style.flexDirection = FlexDirection.Row;

            var typeContainer = new VisualElement();
            typeContainer.style.flexDirection = FlexDirection.Row;
            typeContainer.AddActionTypeContainer(dialogueAction.Action);
                
            var actionTypeField = new EnumField(dialogueAction.Action.Type);
            actionTypeField.RegisterValueChangedCallback(evt =>
            {
                dialogueAction.Action = QuickGetType((ActionType)evt.newValue);
                typeContainer.Clear();
                typeContainer.AddActionTypeContainer(dialogueAction.Action);
            });
            actionTypeField.style.display = DisplayStyle.Flex;

            reqContent.Add(actionTypeField);
            reqContent.Add(typeContainer);

            return reqContent;
        }
    }
}