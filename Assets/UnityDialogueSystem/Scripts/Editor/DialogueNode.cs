using System;
using System.Collections.Generic;
using System.Linq;
using UnityDialogueSystem.Scripts.Actions;
using UnityDialogueSystem.Scripts.Editor.CustomElements;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityDialogueSystem.Scripts.Editor
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

        public bool TryGetOutputEdges(out IEnumerable<Edge> edges, GraphView graphView)
        {
            edges = graphView.edges.Where(e => e.output == OutputPort);
            return edges.Any();
        }

        public Port GeneratePort(Direction portDirection, string portName, Port.Capacity capacity = Port.Capacity.Multi)
        {
            Port port = this.InstantiatePort(Orientation.Horizontal, portDirection, capacity, typeof(float));
            port.portName = portName;
            port.portType = typeof(float);

            VisualElement container = portDirection == Direction.Input ? inputContainer : outputContainer;
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
            VisualElement borderEl = new();
            borderEl.style.flexDirection = direction;

            if (bgColored)
            {
                borderEl.style.backgroundColor = color;
            }

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

        private static VisualElement GetItemOlayContainer(ItemOlay olay)
        {
            VisualElement content = new();
            content.style.flexDirection = FlexDirection.Row;

            EnumField itemField = new(olay.ItemName);
            itemField.RegisterValueChangedCallback(evt => { olay.ItemName = (ItemName)evt.newValue; });
            content.Add(itemField);

            EnumField typeField = new(olay.ItemOlayType);
            typeField.RegisterValueChangedCallback(evt => { olay.ItemOlayType = (ItemOlayType)evt.newValue; });
            content.Add(typeField);

            return content;
        }

        private static VisualElement GetPlayerOlayContainer(PlayerOlay olay)
        {
            VisualElement content = new();
            content.style.flexDirection = FlexDirection.Row;

            EnumField typeField = new(olay.PlayerOlayType);
            typeField.RegisterValueChangedCallback(evt => { olay.PlayerOlayType = (PlayerOlayType)evt.newValue; });
            content.Add(typeField);

            return content;
        }

        private static VisualElement GetLocationOlayContainer(LocationOlay olay)
        {
            VisualElement content = new();
            content.style.flexDirection = FlexDirection.Row;

            EnumField typeField = new(olay.LocationOlayType);
            typeField.RegisterValueChangedCallback(evt => { olay.LocationOlayType = (LocationOlayType)evt.newValue; });
            content.Add(typeField);

            return content;
        }

        private static VisualElement GetMusicOlayContainer(MusicOlay olay)
        {
            VisualElement content = new();

            EnumField typeField = new(olay.MusicOlayType);
            typeField.RegisterValueChangedCallback(evt => { olay.MusicOlayType = (MusicOlayType)evt.newValue; });
            content.Add(typeField);

            return content;
        }

        private static VisualElement GetObjectOlayContainer(ObjectOlay olay)
        {
            VisualElement content = new();
            content.style.flexDirection = FlexDirection.Row;

            EnumField typeField = new(olay.ObjectOlayType);
            typeField.RegisterValueChangedCallback(evt => { olay.ObjectOlayType = (ObjectOlayType)evt.newValue; });
            content.Add(typeField);

            ObjectField objectField = new()
            {
                objectType = typeof(GameObject),
                value = olay.TargetObject
            };
            objectField.RegisterValueChangedCallback(evt => { olay.TargetObject = evt.newValue as GameObject; });
            content.Add(objectField);

            return content;
        }

        private static void AddOlayTypeContainer(this VisualElement container, Olay olay)
        {
            switch (olay)
            {
                case ItemOlay itemOlay:
                    container.Add(GetItemOlayContainer(itemOlay));
                    break;

                case PlayerOlay playerOlay:
                    container.Add(GetPlayerOlayContainer(playerOlay));
                    break;

                case LocationOlay locationOlay:
                    container.Add(GetLocationOlayContainer(locationOlay));
                    break;

                case MusicOlay musicOlay:
                    container.Add(GetMusicOlayContainer(musicOlay));
                    break;

                case ObjectOlay objectOlay:
                    container.Add(GetObjectOlayContainer(objectOlay));
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(olay));
            }
        }

        private static Olay QuickGetType(OlayType type)
        {
            return type.CreateDefault();
        }

        private static void AddConditionDetailsBox(this VisualElement detailBox, ConditionSet conditions)
        {
            detailBox.style.flexDirection = FlexDirection.Row;
            detailBox.style.alignItems = Align.Center;
            detailBox.style.alignSelf = Align.Center;
            detailBox.style.paddingTop = 5f;
            detailBox.style.paddingBottom = 5f;

            Label labelLoc = new(" Location : ");
            detailBox.Add(labelLoc);

            EnumField locationField = new(conditions.Location);
            locationField.RegisterValueChangedCallback(evt => { conditions.Location = (Location)evt.newValue; });
            locationField.style.width = 100f;
            detailBox.Add(locationField);

            Label labelPersist = new(" permanent ");
            detailBox.Add(labelPersist);

            Toggle togglePersist = new()
            {
                value = conditions.Persistent
            };
            togglePersist.RegisterValueChangedCallback(evt => { conditions.Persistent = evt.newValue; });
            detailBox.Add(togglePersist);

            Label labelCut = new(" cut ");
            detailBox.Add(labelCut);

            Toggle toggleCut = new()
            {
                value = conditions.Cut
            };
            toggleCut.RegisterValueChangedCallback(evt => { conditions.Cut = evt.newValue; });
            detailBox.Add(toggleCut);
        }

        public static VisualElement GetConditionBox(ConditionSet conditions, bool hasDetails)
        {
            conditions ??= new ConditionSet();

            return GetOlaySetBox(
                conditions,
                " -   CONDITIONS   - ",
                new Color(0.98f, 0.46f, 0.46f),
                hasDetails,
                detailBox => detailBox.AddConditionDetailsBox(conditions));
        }

        public static VisualElement GetTriggerBox(TriggerSet triggers)
        {
            triggers ??= new TriggerSet();

            return GetOlaySetBox(
                triggers,
                " -   TRIGGERS   - ",
                new Color(0.37f, 0.99f, 1f),
                false,
                null);
        }

        private static VisualElement GetOlaySetBox(
            OlaySet olaySet,
            string title,
            Color color,
            bool hasDetails,
            Action<VisualElement> addDetailContent)
        {
            VisualElement allBox = new();
            allBox.style.flexDirection = FlexDirection.Column;
            allBox.style.alignSelf = Align.Stretch;

            VisualElement headerBox = new();
            headerBox.style.flexDirection = FlexDirection.Row;
            headerBox.style.alignItems = Align.Center;
            headerBox.style.alignSelf = Align.Center;
            headerBox.style.marginTop = 10f;
            headerBox.style.marginBottom = 10f;

            VisualElement detailBox = new();
            detailBox.style.flexDirection = FlexDirection.Row;
            detailBox.style.alignItems = Align.Center;
            detailBox.style.alignSelf = Align.Center;
            detailBox.style.display = hasDetails ? DisplayStyle.Flex : DisplayStyle.None;

            VisualElement olaysBox = new();
            olaysBox.style.flexDirection = FlexDirection.Column;
            olaysBox.style.display = olaySet.IsInitializedForDebug() ? DisplayStyle.Flex : DisplayStyle.None;

            Label label = new(title);
            label.style.unityFontStyleAndWeight = FontStyle.Bold;
            label.style.alignContent = Align.Center;
            label.style.color = color;
            headerBox.Add(label);

            foreach (Olay olay in olaySet.Get())
            {
                olaysBox.AddOlayBox(olaySet, olay, color);
            }

            Button addButton = new(() =>
            {
                Olay olay = olaySet.Add(new ItemOlay());

                if (olay == null)
                {
                    return;
                }

                olaysBox.AddOlayBox(olaySet, olay, color);
                olaysBox.style.display = DisplayStyle.Flex;

                if (hasDetails)
                {
                    detailBox.style.display = DisplayStyle.Flex;
                }

                RefreshListIndices(olaysBox);
            });

            addButton.text = " + ";
            addButton.style.alignItems = Align.FlexEnd;
            addButton.style.unityFontStyleAndWeight = FontStyle.Bold;
            addButton.style.color = color;
            headerBox.Add(addButton);

            if (hasDetails && addDetailContent != null)
            {
                addDetailContent.Invoke(detailBox);
            }

            allBox.Add(headerBox);
            allBox.Add(detailBox);
            allBox.Add(olaysBox);

            Color borderColor = new(color.r, color.g, color.b, 0.42f);
            allBox.DrawFrameAround(borderColor);

            return allBox;
        }

        private static void AddOlayBox(this VisualElement container, OlaySet olaySet, Olay olay, Color color)
        {
            if (olaySet.Get() == null || olaySet.Count == 0 || olay == null)
            {
                return;
            }

            int index = olaySet.Get().IndexOf(olay) + 1;

            container.AddListOlayBox(olaySet, olay, color, index, () =>
            {
                olaySet.Remove(olay);
                container.style.display = olaySet.IsInitializedForDebug() ? DisplayStyle.Flex : DisplayStyle.None;
                RefreshListIndices(container);
            });
        }

        public static void AddListOlayBox(this VisualElement container, OlaySet olaySet, Olay olay, Color color,
            int index, Action onDelete)
        {
            VisualElement olayContainer = new();
            olayContainer.userData = new OlayContainerContext(olaySet, olay);
            olayContainer.style.flexDirection = FlexDirection.Row;
            olayContainer.style.paddingTop = 5;
            olayContainer.style.paddingBottom = 5;
            olayContainer.style.paddingLeft = 5;
            olayContainer.style.paddingRight = 5;

            Label label = new($"  #:{index} ")
            {
                name = "index-label"
            };
            label.style.unityTextAlign = TextAnchor.MiddleLeft;
            label.style.fontSize = 10;
            label.style.color = color;
            olayContainer.Add(label);

            olayContainer.AddOlayBoxInternal();

            Button deleteButton = new(() =>
            {
                onDelete.Invoke();
                container.Remove(olayContainer);
            });

            deleteButton.text = "x";
            deleteButton.style.alignItems = Align.Center;
            deleteButton.style.color = color;

            olayContainer.Add(deleteButton);
            olayContainer.DrawFrameAround(color);

            olayContainer.style.marginLeft = 10f;
            olayContainer.style.marginRight = 10f;
            olayContainer.style.marginTop = 4f;
            olayContainer.style.marginBottom = 4f;

            container.Add(olayContainer);
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

        private static void AddOlayBoxInternal(this VisualElement container)
        {
            if (container.userData is not OlayContainerContext context || context.Olay == null)
            {
                return;
            }

            EnumField olayTypeField = new(context.Olay.Type);
            VisualElement typeContainer = new();
            typeContainer.style.flexDirection = FlexDirection.Row;
            typeContainer.AddOlayTypeContainer(context.Olay);

            olayTypeField.RegisterValueChangedCallback(evt =>
            {
                Olay newOlay = QuickGetType((OlayType)evt.newValue);

                int index = context.OlaySet.Get().IndexOf(context.Olay);
                if (index < 0)
                {
                    return;
                }

                context.OlaySet.Get()[index] = newOlay;
                context.Olay = newOlay;

                typeContainer.Clear();
                typeContainer.AddOlayTypeContainer(context.Olay);
            });

            container.Add(olayTypeField);
            container.Add(typeContainer);
        }

        private static void RefreshListIndices(VisualElement listContainer)
        {
            int visibleIndex = 1;

            foreach (VisualElement child in listContainer.Children())
            {
                Label indexLabel = child.Q<Label>("index-label");
                if (indexLabel == null)
                {
                    continue;
                }

                indexLabel.text = $"  #:{visibleIndex} ";
                visibleIndex++;
            }
        }

        private sealed class OlayContainerContext
        {
            public OlaySet OlaySet;
            public Olay Olay;

            public OlayContainerContext(OlaySet olaySet, Olay olay)
            {
                OlaySet = olaySet;
                Olay = olay;
            }
        }
    }
}