using System;
using UnityEngine;

namespace UnityDialogueSystem.Scripts.Actions
{
    [Serializable]
    public abstract class Olay
    {
        public abstract OlayType Type { get; }

        public virtual string Title => Type.GetDisplayName();

        public virtual Color Color => Type.GetColor();

        public abstract bool Matches(Olay other);

        public abstract Olay Clone();
    }

    [Serializable]
    public sealed class ItemOlay : Olay
    {
        [SerializeField]
        private ItemOlayType m_ItemOlayType;

        [SerializeField]
        private ItemName m_ItemName;

        public override OlayType Type => OlayType.Item;


        public ItemOlayType ItemOlayType
        {
            get => m_ItemOlayType;
            set => m_ItemOlayType = value;
        }

        public ItemName ItemName
        {
            get => m_ItemName;
            set => m_ItemName = value;
        }

        public ItemOlay()
        {
        }

        public ItemOlay(ItemOlayType itemOlayType, ItemName itemName)
        {
            m_ItemOlayType = itemOlayType;
            m_ItemName = itemName;
        }

        public override bool Matches(Olay other)
        {
            if (other is not ItemOlay itemOlay)
            {
                return false;
            }

            if (m_ItemOlayType != itemOlay.m_ItemOlayType)
            {
                return false;
            }

            return m_ItemName == itemOlay.m_ItemName;
        }

        public override Olay Clone()
        {
            return new ItemOlay(m_ItemOlayType, m_ItemName);
        }
    }

    [Serializable]
    public sealed class PlayerOlay : Olay
    {
        [SerializeField]
        private PlayerOlayType m_PlayerOlayType;

        public override OlayType Type => OlayType.Player;


        public PlayerOlayType PlayerOlayType
        {
            get => m_PlayerOlayType;
            set => m_PlayerOlayType = value;
        }

        public PlayerOlay()
        {
        }

        public PlayerOlay(PlayerOlayType playerOlayType)
        {
            m_PlayerOlayType = playerOlayType;
        }

        public override bool Matches(Olay other)
        {
            if (other is not PlayerOlay playerOlay)
            {
                return false;
            }

            if (m_PlayerOlayType != playerOlay.m_PlayerOlayType)
            {
                return false;
            }

            return true;
        }

        public override Olay Clone()
        {
            return new PlayerOlay(m_PlayerOlayType);
        }
    }

    [Serializable]
    public sealed class MusicOlay : Olay
    {
        [SerializeField]
        private MusicOlayType m_MusicOlayType;

        public override OlayType Type => OlayType.Music;


        public MusicOlayType MusicOlayType
        {
            get => m_MusicOlayType;
            set => m_MusicOlayType = value;
        }

        public MusicOlay()
        {
        }

        public MusicOlay(MusicOlayType musicOlayType)
        {
            m_MusicOlayType = musicOlayType;
        }

        public override bool Matches(Olay other)
        {
            if (other is not MusicOlay musicOlay)
            {
                return false;
            }

            if (m_MusicOlayType != musicOlay.m_MusicOlayType)
            {
                return false;
            }

            return true;
        }

        public override Olay Clone()
        {
            return new MusicOlay(m_MusicOlayType);
        }
    }

    [Serializable]
    public sealed class ObjectOlay : Olay
    {
        [SerializeField]
        private ObjectOlayType m_ObjectOlayType;

        [SerializeField]
        private GameObject m_GameObject;

        public override OlayType Type
        {
            get { return OlayType.Object; }
        }


        public ObjectOlayType ObjectOlayType
        {
            get => m_ObjectOlayType;
            set => m_ObjectOlayType = value;
        }

        public GameObject TargetObject
        {
            get => m_GameObject;
            set => m_GameObject = value;
        }

        public ObjectOlay()
        {
        }

        public ObjectOlay(ObjectOlayType objectOlayType, GameObject targetObject = null)
        {
            m_ObjectOlayType = objectOlayType;
            m_GameObject = targetObject;
        }

        public override bool Matches(Olay other)
        {
            if (other is not ObjectOlay objectOlay)
            {
                return false;
            }

            if (m_ObjectOlayType != objectOlay.m_ObjectOlayType)
            {
                return false;
            }

            if (m_GameObject != objectOlay.m_GameObject)
            {
                return false;
            }

            return true;
        }

        public override Olay Clone()
        {
            return new ObjectOlay(m_ObjectOlayType, m_GameObject);
        }
    }

    [Serializable]
    public sealed class LocationOlay : Olay
    {
        [SerializeField]
        private LocationOlayType m_LocationOlayType;

        public override OlayType Type => OlayType.Location;


        public LocationOlayType LocationOlayType
        {
            get => m_LocationOlayType;
            set => m_LocationOlayType = value;
        }

        public LocationOlay()
        {
        }

        public LocationOlay(LocationOlayType locationOlayType)
        {
            m_LocationOlayType = locationOlayType;
        }

        public override bool Matches(Olay other)
        {
            if (other is not LocationOlay locationOlay)
            {
                return false;
            }

            if (m_LocationOlayType != locationOlay.m_LocationOlayType)
            {
                return false;
            }

            return true;
        }

        public override Olay Clone()
        {
            return new LocationOlay(m_LocationOlayType);
        }
    }
}