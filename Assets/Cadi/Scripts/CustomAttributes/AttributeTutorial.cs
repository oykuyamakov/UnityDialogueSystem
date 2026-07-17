using UnityEngine;

namespace Cadi.Scripts.CustomAttributes
{
    public class AttributeTutorial : MonoBehaviour
    {
        [SerializeField]
        private bool m_Boolean = false;

        [SerializeField, ShowIf(nameof(m_Boolean))]
        private int m_ShowIfBooleanTrue = 0;

        [SerializeField, SpritePreview]
        private Sprite m_SpriteWithAPreview;

        [SerializeField, SpritePreview]
        private Sprite m_AnotherSpriteWithAPreview;

        [Button("Custom name for function that toggles boolean")]
        public void FunctionThatTogglesBoolean()
        {
            m_Boolean = !m_Boolean;
        }

        [SerializeField, Range(0, 5)]
        private int m_MaxRange;

        [SerializeField, Range(5, 10)]
        private int m_MinRange;

        [SerializeField, DynamicRange(nameof(m_MinRange), nameof(m_MaxRange))]
        private int m_ValueInRange;
    }
}