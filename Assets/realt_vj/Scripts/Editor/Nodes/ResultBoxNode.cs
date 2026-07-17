using System;
using System.Collections.Generic;
using RealtVJ.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealtVJ.Editor
{
    public class ResultBoxNode : VJNode
    {
        public ResultBoxData Data { get; }
        public Port DefaultStatePort { get; }

        private VisualElement m_ResultsContainer;

        public ResultBoxNode(string guid, ResultBoxData data) : base(guid, "RESULT")
        {
            Data = data;
            AddToClassList("result-box");

            TopPort = AddInputPort("In");
            BottomPort = AddOutputPort("Out");

            // Default state port (horizontal, right side)
            DefaultStatePort = AddSideOutputPort("Default");
            DefaultStatePort.portColor = new Color(0.49f, 0.23f, 0.93f);

            SetupContentBox();
            BuildUI();
        }

        private void BuildUI()
        {
            var settingsHeader = new Label("SETTINGS");
            settingsHeader.AddToClassList("section-header");
            settingsHeader.AddToClassList("result-section-header");
            ContentBox.Add(settingsHeader);

            // Execution mode
            var modeField = new EnumField(Data.ExecutionMode);
            modeField.RegisterValueChangedCallback(evt => Data.ExecutionMode = (ResultExecutionMode)evt.newValue);
            ContentBox.Add(ResultUIBuilder.CreateFieldRow("Execution", modeField));

            // Loop count
            var loopField = new IntegerField { value = Data.LoopCount };
            loopField.RegisterValueChangedCallback(evt => Data.LoopCount = evt.newValue);
            ContentBox.Add(ResultUIBuilder.CreateFieldRow("Loops", loopField));

            // Results section
            var resultHeader = new Label("RESULTS");
            resultHeader.AddToClassList("section-header");
            resultHeader.AddToClassList("result-section-header");
            resultHeader.style.marginTop = 8;
            ContentBox.Add(resultHeader);

            m_ResultsContainer = new VisualElement();
            ContentBox.Add(m_ResultsContainer);

            foreach (var result in Data.Results)
                AddResultUI(result);

            var addButton = new Button(OnAddResultClicked) { text = "+ Add Result" };
            addButton.AddToClassList("add-btn");
            addButton.AddToClassList("add-btn-result");
            ContentBox.Add(addButton);
        }

        private void OnAddResultClicked()
        {
            var names = ResultTypeRegistry.GetDisplayNames();
            if (names.Count == 0) return;
            var result = ResultTypeRegistry.CreateInstance(ResultTypeRegistry.GetTypeByDisplayName(names[0]));
            Data.AddResult(result);
            AddResultUI(result);
        }

        private void AddResultUI(Result result)
        {
            var container = new VisualElement();
            container.AddToClassList("result-item");

            var headerRow = new VisualElement();
            headerRow.AddToClassList("result-item-header");

            // Type dropdown using registry
            var displayNames = ResultTypeRegistry.GetDisplayNames();
            var currentName = ResultTypeRegistry.GetDisplayName(result.GetType());
            var typePopup = new PopupField<string>(displayNames, currentName);
            typePopup.RegisterValueChangedCallback(evt =>
            {
                int index = IndexOfChild(m_ResultsContainer, container);
                if (index < 0) return;

                var newType = ResultTypeRegistry.GetTypeByDisplayName(evt.newValue);
                if (newType == null) return;

                var newResult = ResultTypeRegistry.CreateInstance(newType);
                Data.ReplaceResultAt(index, newResult);

                var detail = container.Q("result-detail");
                if (detail != null)
                {
                    detail.Clear();
                    ResultUIBuilder.BuildFields(detail, newResult);
                }
            });
            headerRow.Add(typePopup);

            var deleteButton = new Button(() =>
            {
                int index = IndexOfChild(m_ResultsContainer, container);
                if (index >= 0) Data.RemoveResultAt(index);
                m_ResultsContainer.Remove(container);
            }) { text = "X" };
            deleteButton.AddToClassList("delete-btn");
            headerRow.Add(deleteButton);

            container.Add(headerRow);

            var detail = new VisualElement { name = "result-detail" };
            ResultUIBuilder.BuildFields(detail, result);
            container.Add(detail);

            m_ResultsContainer.Add(container);
        }

        private static int IndexOfChild(VisualElement parent, VisualElement child)
        {
            int i = 0;
            foreach (var c in parent.Children()) { if (c == child) return i; i++; }
            return -1;
        }
    }
}
