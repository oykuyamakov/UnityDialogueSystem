using System;
using UnityEngine;

namespace UnityDialogueSystem.Scripts.Actions
{
    [Serializable]
    public enum OlayType
    {
        Item,
        Player,
        Location,
        Music,
        Object
    }

    [Serializable]
    public enum LocationOlayType
    {
        At,
        OnEnter,
        OnExit,
    }

    [Serializable]
    public enum ItemOlayType
    {
        Hold,
        Own,
        Give,
        Take,
        Use,
        Pay,
        Highlight,
        Ping,
        Unlock,
    }

    [Serializable]
    public enum PlayerOlayType
    {
        Any,
        Away,
        Present,
        Sit,
        GetUp,
        LookAt,
    }

    [Serializable]
    public enum MusicOlayType
    {
        Play,
        Pause,
        Next,
        Previous,
        IsPlaying
    }

    [Serializable]
    public enum ObjectOlayType
    {
        Highlight,
        LookAt,
        Disable,
        Enable,
    }

    [Serializable]
    public enum AnimationAction
    {
        Default = 0,
        Idle,
    }

    [Serializable]
    public enum ActionStatus
    {
        Requested,
        Fulfilled,
        Failed
    }

    [Serializable]
    public enum NpcName
    {
        Any = 1,
        Barker = 2,
        //NpcX..
    }

    [Serializable]
    public enum ItemName
    {
        None = 0,
        Any = 1,
        Beer = 2,
        Phone = 4,
        //ItemX...
    }

    [Serializable]
    public enum Location
    {
        Any = 1,
        Home = 2,
        //PlaceX...
    }

    public static class OlayExtensions
    {
        public static Color GetNpcColor(this NpcName npcName)
        {
            return npcName switch
            {
                _ => new Color(1f, 1f, 1f)
            };
        }

        public static Olay CreateDefault(this OlayType olayType)
        {
            switch (olayType)
            {
                case OlayType.Item:
                    return new ItemOlay();
                case OlayType.Player:
                    return new PlayerOlay();
                case OlayType.Location:
                    return new LocationOlay();
                case OlayType.Music:
                    return new MusicOlay();
                case OlayType.Object:
                    return new ObjectOlay();
                default:
                    throw new ArgumentOutOfRangeException(nameof(olayType), olayType, null);
            }
        }

        public static string GetDisplayName(this OlayType olayType)
        {
            switch (olayType)
            {
                case OlayType.Item:
                    return "Item";
                case OlayType.Player:
                    return "Player";
                case OlayType.Location:
                    return "Location";
                case OlayType.Music:
                    return "Music";
                case OlayType.Object:
                    return "Object";
                default:
                    throw new ArgumentOutOfRangeException(nameof(olayType), olayType, null);
            }
        }

        public static Color GetColor(this OlayType olayType)
        {
            switch (olayType)
            {
                case OlayType.Item:
                    return new Color(0.67f, 0.99f, 1f);
                case OlayType.Player:
                    return new Color(0.99f, 0.69f, 1f);
                case OlayType.Location:
                    return new Color(1f, 0.67f, 0.4f);
                case OlayType.Music:
                    return new Color(1f, 0.42f, 0.83f);
                case OlayType.Object:
                    return new Color(1f, 1f, 0.47f);
                default:
                    return Color.white;
            }
        }
    }
}