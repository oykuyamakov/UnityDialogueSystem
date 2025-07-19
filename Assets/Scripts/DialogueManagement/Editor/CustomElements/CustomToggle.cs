using UnityEngine.UIElements;

namespace DialogueManagement.Editor.CustomElements
{
    public class CustomToggle : Toggle
    {
        public CustomToggle(string txt)
        {
            AddToClassList(txt);
        }
        
    }
}
