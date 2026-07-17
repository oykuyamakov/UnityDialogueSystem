using System;
using System.Collections.Generic;
using RealtVJ.Data;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UIElements;

namespace RealtVJ.Editor
{
    public class DefaultStateNode : VJNode
    {
        public DefaultStateBoxData Data { get; }

        private VisualElement m_ResultsContainer;

        public DefaultStateNode(string guid, DefaultStateBoxData data) : base(guid, "DEFAULT STATE")
        {
            Data = data;
            AddToClassList("default-state-box");

            // Horizontal input port on left side (connects from ResultBox's DefaultStatePort)
            TopPort = AddSideInputPort("From Result");

            SetupContentBox();
            BuildUI();
        }

        private void BuildUI()
        {
            var header = new Label("DEFAULT VALUES");
            header.AddToClassList("section-header");
            header.style.color = new Color(0.65f, 0.55f, 0.98f);
            header.style.backgroundColor = new Color(0.12f, 0.08f, 0.25f);
            ContentBox.Add(header);

            m_ResultsContainer = new VisualElement();
            ContentBox.Add(m_ResultsContainer);

            foreach (var result in Data.DefaultResults)
                AddDefaultResultUI(result);

            var addButton = new Button(OnAddClicked) { text = "+ Add Default" };
            addButton.AddToClassList("add-btn");
            addButton.AddToClassList("add-btn-default");
            ContentBox.Add(addButton);
        }

        private void OnAddClicked()
        {
            var names = ResultTypeRegistry.GetDisplayNames();
            if (names.Count == 0) return;
            var result = ResultTypeRegistry.CreateInstance(ResultTypeRegistry.GetTypeByDisplayName(names[0]));
            Data.AddDefaultResult(result);
            AddDefaultResultUI(result);
        }

        private void AddDefaultResultUI(Result result)
        {
            var container = new VisualElement();
            container.AddToClassList("result-item");
            container.style.borderLeftColor = new Color(0.49f, 0.23f, 0.93f);
            container.style.borderLeftWidth = 2;

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
                Data.ReplaceDefaultResultAt(index, newResult);

                var detail = container.Q("default-detail");
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
                if (index >= 0) Data.RemoveDefaultResultAt(index);
                m_ResultsContainer.Remove(container);
            }) { text = "X" };
            deleteButton.AddToClassList("delete-btn");
            headerRow.Add(deleteButton);

            container.Add(headerRow);

            var detail = new VisualElement { name = "default-detail" };
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
