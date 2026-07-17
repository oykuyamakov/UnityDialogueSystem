using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public sealed class HandTrigger : Trigger
    {
        [SerializeField]
        private WhichHand m_Which;

        [SerializeField]
        private HandPosition m_LeftPosition = HandPosition.Anywhere;

        [SerializeField]
        private HandPosition m_RightPosition = HandPosition.Anywhere;

        [SerializeField]
        private HandGesture m_Gesture = HandGesture.Any;

        public override TriggerType Type => TriggerType.Hand;

        public WhichHand Which
        {
            get => m_Which;
            set => m_Which = value;
        }

        public HandPosition LeftPosition
        {
            get => m_LeftPosition;
            set => m_LeftPosition = value;
        }

        /// <summary>
        /// Only used when Which is LeftAndRight.
        /// </summary>
        public HandPosition RightPosition
        {
            get => m_RightPosition;
            set => m_RightPosition = value;
        }

        public HandGesture Gesture
        {
            get => m_Gesture;
            set => m_Gesture = value;
        }

        public HandTrigger() { }

        public HandTrigger(WhichHand which, HandPosition leftPosition = HandPosition.Anywhere,
            HandPosition rightPosition = HandPosition.Anywhere, HandGesture gesture = HandGesture.Any)
        {
            m_Which = which;
            m_LeftPosition = leftPosition;
            m_RightPosition = rightPosition;
            m_Gesture = gesture;
        }

        public override Trigger Clone()
        {
            return new HandTrigger(m_Which, m_LeftPosition, m_RightPosition, m_Gesture);
        }
    }
}
