using DialogueManagement.Actions;
using DialogueManagement.Data;
using UnityEngine;
using UnityEngine.UIElements;

namespace DialogueManagement.Editor.CustomElements
{
    public abstract class LineBox : Box
    {
        public abstract Line GetLine();
        public abstract DialogueNode GetNode();

        protected VisualElement m_BaseContent;

        protected VisualElement m_RequirementsContent;

        protected VisualElement m_AllContent;
        
        protected VisualElement m_NodeSpecificContent;

        protected abstract bool HasSpecialReq();

        protected virtual void AddTextBox()
        {
            m_BaseContent.Add(GetDetailsToggle());
            m_BaseContent.Add(GetTextField());
        }

        protected virtual void AddSettingsBox()
        {
            m_RequirementsContent.Add(GraphNodeExtensions.GetConditionBox(GetLine().DialogueConditionContainer, HasSpecialReq()));
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
            
            m_NodeSpecificContent = new VisualElement();
            m_NodeSpecificContent.style.flexDirection = FlexDirection.Column;
            
            m_NodeSpecificContent.style.backgroundColor = color;
            m_NodeSpecificContent.style.borderTopWidth = 5f;
            m_NodeSpecificContent.style.borderBottomWidth = 5f;
            m_NodeSpecificContent.style.borderLeftWidth = 5f;
            m_NodeSpecificContent.style.borderRightWidth = 5f;
            m_NodeSpecificContent.style.borderTopColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_NodeSpecificContent.style.borderBottomColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_NodeSpecificContent.style.borderLeftColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_NodeSpecificContent.style.borderRightColor = new Color(0.2f, 0.2f, 0.2f, 0.76f);
            m_NodeSpecificContent.style.paddingBottom = 5;
            
            m_RequirementsContent = new VisualElement();
            m_RequirementsContent.style.flexDirection = FlexDirection.Column;
            m_RequirementsContent.style.alignItems = Align.FlexStart;
            m_RequirementsContent.style.paddingTop = 2;
            m_RequirementsContent.style.paddingBottom = 2;
            m_RequirementsContent.style.display = DisplayStyle.Flex;
            
            AddTextBox();
            AddSettingsBox();
            
            m_AllContent.Add(m_BaseContent);
            m_AllContent.Add(m_NodeSpecificContent);
            m_AllContent.Add(m_RequirementsContent);

            Add(m_AllContent);

            GetNode().NodeContentContainer.Add(this);

            GetNode().RefreshExpandedState();
            GetNode().RefreshPorts();
        }


        protected virtual TextField GetTextField()
        {
            var textField = new TextField(string.Empty);
            textField.RegisterValueChangedCallback(evt => { GetLine().DialogueLine = evt.newValue; });
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
            var toggle = new CustomToggle("settings-toggle");
            toggle.RegisterValueChangedCallback(evt =>
            {
                {
                    GetLine().m_ShowSettings = evt.newValue;
                    var showSettings = GetLine().m_ShowSettings || HasSpecialReq() &&(GetLine().DialogueConditionContainer.Location != Location.Any) || GetLine()
                        .DialogueConditionContainer.DebugIsInitialized();
                    m_RequirementsContent.style.display = showSettings ? DisplayStyle.Flex : DisplayStyle.None;

                    var cl = GetLine().m_ShowSettings
                        ? new Color(0.41f, 0.25f, 0.4f, 0.78f)
                        : new Color(0.09f, 0.64f, 0.55f, 0.78f);
                    toggle.style.borderBottomColor = cl;
                    toggle.style.borderTopColor = cl;
                    toggle.style.borderLeftColor = cl;
                    toggle.style.borderRightColor = cl;
                }
            });
            
            toggle.style.flexShrink = 1;
            toggle.tooltip = "Enable Details";
            
            var showSettings = GetLine().m_ShowSettings || HasSpecialReq() &&(GetLine().DialogueConditionContainer.Location != Location.Any) || GetLine()
                .DialogueConditionContainer.DebugIsInitialized();
            m_RequirementsContent.style.display = showSettings ? DisplayStyle.Flex : DisplayStyle.None;

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