using System;
//using Sirenix.OdinInspector;
using UnityEngine;

namespace DialogueManagement.Actions
{
    [Serializable]
    public abstract class ActionRef
    {
        //[ReadOnly] [GUIColor(nameof(GetColor))]
        public ActionType Type => m_ActionType;

        protected ActionType m_ActionType;

        public abstract string GetTitle();
        public abstract Color GetColor();
    }

    [Serializable]
    public class ItemAction : ActionRef
    {
        [SerializeField]
        //[InlineProperty] [GUIColor(nameof(GetColor))]
        private ItemActionType m_ItemActionType;

        public ItemActionType ItemActionType
        {
            get => m_ItemActionType;
            set => m_ItemActionType = value;
        }

        [SerializeField] 
        //[InlineProperty] [GUIColor(nameof(GetColor))]
        private ItemName m_ItemName;

        public ItemName ItemName
        {
            get => m_ItemName;
            set => m_ItemName = value;
        }

        public ItemAction(ItemActionType itemActionType, ItemName itemName)
        {
            m_ItemActionType = itemActionType;
            m_ItemName = itemName;
            m_ActionType = ActionType.Item;
        }

        public ItemAction()
        {
            m_ActionType = ActionType.Item;
        }

        public override string GetTitle()
        {
            return "Item Action: ";
        }

        public override Color GetColor()
        {
            return new Color(0.67f, 0.99f, 1f);
        }
    }

    [Serializable]
    public class PlayerAction : ActionRef
    {
        [SerializeField]
        //[InlineProperty] [GUIColor(nameof(GetColor))]
        private PlayerActionType m_PlayerActionType;

        public PlayerActionType PlayerActionType
        {
            get => m_PlayerActionType;
            set => m_PlayerActionType = value;
        }

        public PlayerAction(PlayerActionType playerActionType)
        {
            m_PlayerActionType = playerActionType;
            m_ActionType = ActionType.Player;
        }

        public PlayerAction()
        {
            m_ActionType = ActionType.Player;
        }

        public override string GetTitle()
        {
            return "Player Action: ";
        }

        public override Color GetColor()
        {
            return new Color(0.99f, 0.69f, 1f);
        }
    }

    [Serializable]
    public class MusicAction : ActionRef
    {
        [SerializeField] 
        //[InlineProperty] [GUIColor(nameof(GetColor))]
        private MusicActionType m_MusicActionType;

        public MusicActionType MusicActionType
        {
            get => m_MusicActionType;
            set => m_MusicActionType = value;
        }

        public MusicAction(MusicActionType musicActionType)
        {
            m_MusicActionType = musicActionType;
            m_ActionType = ActionType.Music;
        }

        public MusicAction()
        {
            m_ActionType = ActionType.Music;
        }

        public override string GetTitle()
        {
            return "Music Action: ";
        }

        public override Color GetColor()
        {
            return new Color(1f, 0.42f, 0.83f);
        }
    }

    [Serializable]
    public class GameObjectAction : ActionRef
    {
        [SerializeField]
        //[InlineProperty] [GUIColor(nameof(GetColor))]
        private GOActionType m_GOActionType;

        public GOActionType GOActionType
        {
            get => m_GOActionType;
            set => m_GOActionType = value;
        }

        [SerializeField] 
        //[GUIColor(nameof(GetColor))]
        public GameObject m_GameObject;

        public GameObjectAction(GOActionType goActionType)
        {
            m_ActionType = ActionType.GameObject;
            m_GOActionType = goActionType;
        }

        public GameObjectAction()
        {
            m_ActionType = ActionType.GameObject;
        }

        public override string GetTitle()
        {
            return "GameObject Action: ";
        }

        public override Color GetColor()
        {
            return new Color(1f, 1f, 0.47f);
        }
    }

    [Serializable]
    public class LocationAction : ActionRef
    {
        [SerializeField] 
        //[InlineProperty] [GUIColor(nameof(GetColor))]
        private LocationActionType m_LocationAction;

        public LocationActionType LocationActionType
        {
            get => m_LocationAction;
            set => m_LocationAction = value;
        }

        public LocationAction(LocationActionType locationActionType)
        {
            m_ActionType = ActionType.Location;
            m_LocationAction = locationActionType;
        }

        public LocationAction()
        {
            m_ActionType = ActionType.Location;
        }

        public override string GetTitle()
        {
            return "Location: ";
        }

        public override Color GetColor()
        {
            return new Color(1f, 0.67f, 0.4f);
        }
    }
}