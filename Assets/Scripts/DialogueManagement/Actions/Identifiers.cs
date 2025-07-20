using System;
using System.Collections.Generic;
using UnityEngine;

namespace DialogueManagement.Actions
{
    [Serializable]
    public enum ActionType
    {
        Item,
        Player,
        Location,
        Music,
        GameObject,
    }

    [Serializable]
    public enum LocationActionType
    {
        At,
        OnEnter,
        OnExit,
    }

    [Serializable]
    public enum ItemActionType
    {
        Hold,
        Own,
        Give,
        Take,
        Use,
        Pay,
        Highlight,
        Ping
    }

    [Serializable]
    public enum MusicActionType
    {
        Play,
        Pause,
        Next,
        Previous,
        IsPlaying
    }

    [Serializable]
    public enum PlayerActionType
    {
        Sit,
        GetUp,
    }

    [Serializable]
    public enum GOActionType
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
    public enum NpcName
    {
        Any = 1,
        Barker = 2,
        Shopkeeper = 4,
    }

    [Serializable]
    public enum ItemName
    {
        Any = 1,
        Beer = 2,
    }
    
    [Serializable]
    public enum Location
    {
        Any = 1,
        Home = 2,
    }

    public static class ActionExtensions
    {
        public static Color GetNpcColor(this NpcName npcName)
        {
            return npcName switch
            {
                NpcName.Barker => new Color(0.8f, 0.31f, 0.27f),
                NpcName.Shopkeeper => new Color(0.27f, 0.42f, 0.8f),
                _ => new Color(1f, 1f, 1f) // Default color for Any or unknown
            };
        }
        
        public static ActionRef GetItsClone(this ActionRef original)
        {
            switch (original)
            {
                case ItemAction item:
                    return new ItemAction(item.ItemActionType, item.ItemName);
                case PlayerAction player:
                    return new PlayerAction(player.PlayerActionType);
                case MusicAction music:
                    return new MusicAction(music.MusicActionType);
                case GameObjectAction go:
                    return new GameObjectAction(go.GOActionType)
                    {
                        m_GameObject = go.m_GameObject // !!be careful: GameObject reference stays shared
                    };
                case LocationAction loc:
                    return new LocationAction(loc.LocationActionType);
                default:
                    Debug.LogWarning("Unknown ActionData type in CloneActionData");
                    return null;
            }
        }

        public static bool CheckActionMatch(this ActionRef self, ActionRef triggered)
        {
            if (self == null || triggered == null)
            {
                Debug.LogError("ActionData is null, cannot check match");
                return false;
            }

            if (self.Type != triggered.Type)
                return false;

            switch (self)
            {
                case ItemAction itemReq when triggered is ItemAction itemTriggered &&
                                             itemReq.ItemActionType == itemTriggered.ItemActionType &&
                                             itemReq.ItemName == itemTriggered.ItemName:
                case PlayerAction playerReq when triggered is PlayerAction playerTriggered &&
                                                 playerReq.PlayerActionType == playerTriggered.PlayerActionType &&
                                                 playerReq.PlayerActionType == playerTriggered.PlayerActionType:
                case GameObjectAction goReq when triggered is GameObjectAction goTriggered &&
                                                 goReq.GOActionType == goTriggered.GOActionType &&
                                                 goReq.GOActionType == goTriggered.GOActionType:
                case LocationAction locationReq when triggered is LocationAction locationTriggered &&
                                                     locationReq.LocationActionType == locationTriggered.LocationActionType:
                    return true;
            }

            return false;
        }
    }
}