using System;
using RealtVJ.Data;
using UnityEditor.Experimental.GraphView;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealtVJ.Editor
{
    public class TriggerBoxNode : VJNode
    {
        public TriggerBoxData Data { get; }

        private VisualElement m_TriggersContainer;

        private static readonly Color AccentColor = new(0f, 0.83f, 1f); // #00D4FF

        public TriggerBoxNode(string guid, TriggerBoxData data) : base(guid, "TRIGGER")
        {
            Data = data;
            AddToClassList("trigger-box");

            TopPort = AddInputPort("In");
            BottomPort = AddOutputPort("Out");

            SetupContentBox();
            BuildUI();
        }

        private void BuildUI()
        {
            // -- Settings section --
            var settingsHeader = new Label("SETTINGS");
            settingsHeader.AddToClassList("section-header");
            settingsHeader.AddToClassList("trigger-section-header");
            ContentBox.Add(settingsHeader);

            // Cooldown mode
            var cooldownRow = CreateFieldRow("Cooldown",
                new EnumField(Data.CooldownMode), evt => Data.CooldownMode = (CooldownMode)evt.newValue);
            ContentBox.Add(cooldownRow);

            // Cooldown duration
            var durationField = new FloatField { value = Data.CooldownDuration };
            durationField.RegisterValueChangedCallback(evt => Data.CooldownDuration = evt.newValue);
            var durationRow = CreateFieldRow("Duration", durationField);
            ContentBox.Add(durationRow);

            // -- Triggers section --
            var triggerHeader = new Label("TRIGGERS (AND)");
            triggerHeader.AddToClassList("section-header");
            triggerHeader.AddToClassList("trigger-section-header");
            triggerHeader.style.marginTop = 8;
            ContentBox.Add(triggerHeader);

            m_TriggersContainer = new VisualElement();
            m_TriggersContainer.style.flexDirection = FlexDirection.Column;
            ContentBox.Add(m_TriggersContainer);

            foreach (var trigger in Data.Triggers)
                AddTriggerUI(trigger);

            var addButton = new Button(OnAddTriggerClicked) { text = "+ Add Trigger" };
            addButton.AddToClassList("add-btn");
            addButton.AddToClassList("add-btn-trigger");
            ContentBox.Add(addButton);
        }

        private void OnAddTriggerClicked()
        {
            var trigger = new HandTrigger();
            Data.AddTrigger(trigger);
            AddTriggerUI(trigger);
        }

        private void AddTriggerUI(Trigger trigger)
        {
            var container = new VisualElement();
            container.AddToClassList("trigger-item");

            // Header
            var headerRow = new VisualElement();
            headerRow.AddToClassList("trigger-item-header");

            var typeField = new EnumField(trigger.Type);
            typeField.RegisterValueChangedCallback(evt =>
            {
                int index = IndexOfChild(m_TriggersContainer, container);
                if (index < 0) return;
                var newTrigger = CreateDefaultTrigger((TriggerType)evt.newValue);
                Data.RemoveTriggerAt(index);
                Data.AddTrigger(newTrigger);
                RebuildDetail(container, newTrigger);
            });
            headerRow.Add(typeField);

            var deleteButton = new Button(() =>
            {
                int index = IndexOfChild(m_TriggersContainer, container);
                if (index >= 0) Data.RemoveTriggerAt(index);
                m_TriggersContainer.Remove(container);
            }) { text = "X" };
            deleteButton.AddToClassList("delete-btn");
            headerRow.Add(deleteButton);

            container.Add(headerRow);

            var detail = new VisualElement { name = "trigger-detail" };
            BuildTriggerDetail(detail, trigger);
            container.Add(detail);

            m_TriggersContainer.Add(container);
        }

        private void RebuildDetail(VisualElement container, Trigger trigger)
        {
            var detail = container.Q("trigger-detail");
            if (detail == null) return;
            detail.Clear();
            BuildTriggerDetail(detail, trigger);
        }

        private void BuildTriggerDetail(VisualElement container, Trigger trigger)
        {
            switch (trigger)
            {
                case HandTrigger hand:
                    BuildHandTriggerUI(container, hand);
                    break;
                case MusicTrigger music:
                    container.Add(CreateFieldRow("Type",
                        new EnumField(music.MusicTriggerType),
                        evt => music.MusicTriggerType = (MusicTriggerType)evt.newValue));
                    var valField = new FloatField { value = music.Value };
                    valField.RegisterValueChangedCallback(evt => music.Value = evt.newValue);
                    container.Add(CreateFieldRow("Value", valField));
                    break;
                case TimeTrigger time:
                    container.Add(CreateFieldRow("Type",
                        new EnumField(time.TimeTriggerType),
                        evt => time.TimeTriggerType = (TimeTriggerType)evt.newValue));
                    var durField = new FloatField { value = time.Duration };
                    durField.RegisterValueChangedCallback(evt => time.Duration = evt.newValue);
                    container.Add(CreateFieldRow("Duration", durField));
                    var repField = new IntegerField { value = time.RepeatCount };
                    repField.RegisterValueChangedCallback(evt => time.RepeatCount = evt.newValue);
                    container.Add(CreateFieldRow("Repeat", repField));
                    break;
            }
        }

        private void BuildHandTriggerUI(VisualElement container, HandTrigger hand)
        {
            var whichField = new EnumField(hand.Which);
            whichField.RegisterValueChangedCallback(evt =>
            {
                hand.Which = (WhichHand)evt.newValue;
                var dep = container.Q("hand-dependent");
                if (dep != null)
                {
                    dep.Clear();
                    BuildHandDependentFields(dep, hand);
                }
            });
            container.Add(CreateFieldRow("Which", whichField));

            var dependentContainer = new VisualElement { name = "hand-dependent" };
            BuildHandDependentFields(dependentContainer, hand);
            container.Add(dependentContainer);
        }

        private void BuildHandDependentFields(VisualElement container, HandTrigger hand)
        {
            bool isBoth = hand.Which == WhichHand.LeftAndRight;

            string posLabel = isBoth ? "Left Pos" : "Position";
            var leftPosField = new EnumField(hand.LeftPosition);
            leftPosField.RegisterValueChangedCallback(evt => hand.LeftPosition = (HandPosition)evt.newValue);
            container.Add(CreateFieldRow(posLabel, leftPosField));

            if (isBoth)
            {
                var rightPosField = new EnumField(hand.RightPosition);
                rightPosField.RegisterValueChangedCallback(evt => hand.RightPosition = (HandPosition)evt.newValue);
                container.Add(CreateFieldRow("Right Pos", rightPosField));
            }

            var gestureField = new EnumField(hand.Gesture);
            gestureField.RegisterValueChangedCallback(evt =>
            {
                var g = (HandGesture)evt.newValue;
                if (!isBoth && (g == HandGesture.HandsTogether || g == HandGesture.HandsApart))
                {
                    gestureField.SetValueWithoutNotify(hand.Gesture);
                    return;
                }
                hand.Gesture = g;
            });
            container.Add(CreateFieldRow("Gesture", gestureField));
        }

        // -- Helpers --

        private static VisualElement CreateFieldRow(string label, VisualElement field)
        {
            var row = new VisualElement();
            row.AddToClassList("field-row");
            var lbl = new Label(label);
            lbl.AddToClassList("field-label");
            row.Add(lbl);
            field.style.flexGrow = 1;
            row.Add(field);
            return row;
        }

        private static VisualElement CreateFieldRow<T>(string label, EnumField field,
            EventCallback<ChangeEvent<Enum>> callback) where T : struct
        {
            field.RegisterValueChangedCallback(callback);
            return CreateFieldRow(label, field);
        }

        private static VisualElement CreateFieldRow(string label, EnumField field,
            EventCallback<ChangeEvent<Enum>> callback)
        {
            field.RegisterValueChangedCallback(callback);
            return CreateFieldRow(label, (VisualElement)field);
        }

        private static int IndexOfChild(VisualElement parent, VisualElement child)
        {
            int i = 0;
            foreach (var c in parent.Children())
            {
                if (c == child) return i;
                i++;
            }
            return -1;
        }

        private static Trigger CreateDefaultTrigger(TriggerType type) => type switch
        {
            TriggerType.Hand => new HandTrigger(),
            TriggerType.Music => new MusicTrigger(),
            TriggerType.Time => new TimeTrigger(),
            _ => new HandTrigger()
        };
    }
}
