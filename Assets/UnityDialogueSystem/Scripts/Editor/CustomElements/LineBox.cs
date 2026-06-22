using UnityDialogueSystem.Scripts.Actions;
using UnityDialogueSystem.Scripts.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace UnityDialogueSystem.Scripts.Editor.CustomElements
{
    public abstract class LineBox : Box
    {
        public abstract Line GetLine();
        public abstract DialogueNode GetNode();

        protected VisualElement m_BaseContent;
        protected VisualElement m_SettingsContent;
        protected VisualElement m_AllContent;

        protected abstract bool HasSpecialReq();

        protected virtual void AddTextBox()
        {
            m_BaseContent.Add(GetDetailsToggle());
            m_BaseContent.Add(GetTextField());
        }

        protected virtual void AddSettingsBox()
        {
            if (GetLine().Conditions != null)
            {
                m_SettingsContent.Add(GraphNodeExtensions.GetConditionBox(GetLine().Conditions, HasSpecialReq()));
            }

            if (GetLine().Triggers != null)
            {
                m_SettingsContent.Add(GraphNodeExtensions.GetTriggerBox(GetLine().Triggers));
            }
        }

        protected virtual void InitiateBox(Color color)
        {
            m_AllContent = GraphNodeExtensions.GetBorderBox(FlexDirection.Column, color);

            m_BaseContent = new VisualElement();
            m_BaseContent.style.flexDirection = FlexDirection.Row;
            m_BaseContent.style.backgroundColor = color;
            m_BaseContent.style.borderTopWidth = 5f;
            m_BaseContent.style.borderBottomWidth = 5f;
            m_BaseContent.style.borderLeftWidth = 5f;
            m_BaseContent.style.borderRightWidth = 5f;
            m_BaseContent.style.borderTopColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_BaseContent.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_BaseContent.style.borderLeftColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_BaseContent.style.borderRightColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_BaseContent.style.paddingBottom = 5;

            m_SettingsContent = new VisualElement();
            m_SettingsContent.style.flexDirection = FlexDirection.Column;
            m_SettingsContent.style.alignItems = Align.FlexStart;
            m_SettingsContent.style.paddingTop = 2;
            m_SettingsContent.style.paddingBottom = 2;
            m_SettingsContent.style.display = DisplayStyle.Flex;

            AddTextBox();
            AddSettingsBox();

            m_AllContent.Add(m_BaseContent);
            m_AllContent.Add(m_SettingsContent);

            Add(m_AllContent);

            GetNode().NodeContentContainer.Add(this);
            GetNode().RefreshExpandedState();
            GetNode().RefreshPorts();
        }

        protected virtual TextField GetTextField()
        {
            TextField textField = new(string.Empty);
            textField.RegisterValueChangedCallback(evt =>
            {
                GetLine().DialogueLine = evt.newValue;
            });

            textField.value = GetLine().DialogueLine;
            textField.style.minWidth = 50;
            textField.style.maxWidth = 350;
            textField.style.minHeight = 20;
            textField.style.maxHeight = 600;
            textField.style.unityFontStyleAndWeight = FontStyle.Bold;
            textField.style.display = DisplayStyle.Flex;
            textField.style.flexGrow = 5;
            textField.multiline = true;
            textField.AddToClassList("custom-textfield");

            return textField;
        }

        protected virtual VisualElement GetDetailsToggle()
        {
            CustomToggle toggle = new("settings-toggle");
            toggle.tooltip = "Enable Details";
            toggle.style.flexShrink = 1;

            toggle.RegisterValueChangedCallback(evt =>
            {
                GetLine().m_ShowSettings = evt.newValue;

                bool hasConditionDetails =
                    HasSpecialReq() &&
                    GetLine().Conditions != null &&
                    GetLine().Conditions.Location != Location.Any;

                bool hasConditions = GetLine().Conditions != null && GetLine().Conditions.IsInitializedForDebug();
                bool hasTriggers = GetLine().Triggers != null && GetLine().Triggers.IsInitializedForDebug();

                bool showSettings = GetLine().m_ShowSettings || hasConditionDetails || hasConditions || hasTriggers;
                m_SettingsContent.style.display = showSettings ? DisplayStyle.Flex : DisplayStyle.None;

                Color borderColor = GetLine().m_ShowSettings
                    ? new Color(0.41f, 0.25f, 0.4f, 0.78f)
                    : new Color(0.09f, 0.64f, 0.55f, 0.78f);

                toggle.style.borderBottomColor = borderColor;
                toggle.style.borderTopColor = borderColor;
                toggle.style.borderLeftColor = borderColor;
                toggle.style.borderRightColor = borderColor;
            });

            bool initialHasConditionDetails =
                HasSpecialReq() &&
                GetLine().Conditions != null &&
                GetLine().Conditions.Location != Location.Any;

            bool initialHasConditions = GetLine().Conditions != null && GetLine().Conditions.IsInitializedForDebug();
            bool initialHasTriggers = GetLine().Triggers != null && GetLine().Triggers.IsInitializedForDebug();

            bool initialShowSettings = GetLine().m_ShowSettings || initialHasConditionDetails || initialHasConditions || initialHasTriggers;
            m_SettingsContent.style.display = initialShowSettings ? DisplayStyle.Flex : DisplayStyle.None;

            toggle.style.borderBottomWidth = 1f;
            toggle.style.paddingBottom = 1;
            toggle.style.borderTopWidth = 1;
            toggle.style.paddingTop = 1;
            toggle.style.borderLeftWidth = 1;
            toggle.style.paddingLeft = 1;
            toggle.style.borderRightWidth = 1f;
            toggle.style.paddingRight = 1;
            toggle.value = GetLine().m_ShowSettings;

            return toggle;
        }
    }
}