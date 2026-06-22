using UnityEngine.UIElements;

namespace UnityDialogueSystem.Scripts.Editor.CustomElements
{
    public class CustomToggle : Toggle
    {
        public CustomToggle(string txt)
        {
            AddToClassList(txt);
        }
        
    }
}
