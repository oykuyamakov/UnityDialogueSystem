using System;
using System.Collections.Generic;
using System.Linq;
using UnityDialogueSystem.Scripts.Actions;
using UnityEditor;
using UnityEngine;
//using Sirenix.OdinInspector;
using Object = UnityEngine.Object;

namespace UnityDialogueSystem.Scripts.Data
{
#if UNITY_EDITOR
    public abstract class DialogueContainerExtensions
    {
        [MenuItem("Assets/Create/Create Dialogue From Json")]
        public static void CreateCustomScript()
        {
            Object selectedAsset = Selection.activeObject;

            string path = AssetDatabase.GetAssetPath(selectedAsset);
            DialogueContainer dialogueContainer = DialogueJsonHandler.LoadDialogueFromPath(path);

            Debug.Log(path);
            AssetDatabase.CreateAsset(dialogueContainer, path + selectedAsset.name + ".asset");
            AssetDatabase.SaveAssets();
        }
    }
#endif

    [Serializable]
    public class DialogueContainer : ScriptableObject
    {
        public NpcName NpcName = NpcName.Any;

        [SerializeField]
        // [ListDrawerSettings(DefaultExpandedState = true, ShowFoldout = false, HideAddButton = true,
        //     HideRemoveButton = true)]
        public List<NpcNodeData> NpcNodes = new();

        [SerializeField]
        public List<PlayerNodeData> PlayerNodes = new();

        [SerializeField]
        public List<StartNodeData> StartNodes = new();

        private Dictionary<string, PlayerNodeData> m_PlayerNodesByGuid = new();
        private Dictionary<string, NpcNodeData> m_NpcNodesByGuid = new();
        private Dictionary<string, StartNodeData> m_StartNodesByGuid = new();

        private Dictionary<Location, List<StartNodeData>> m_StartNodesByLocation = new();

        public void ResetRuntimeState()
        {
            foreach (StartNodeData start in StartNodes)
            {
                start.HasPerformed = false;
            }

            RebuildCaches();
        }

        public void InitializeIfReq()
        {
            if (m_StartNodesByGuid.Count > 0 &&
                m_PlayerNodesByGuid.Count > 0 &&
                m_NpcNodesByGuid.Count > 0 &&
                m_StartNodesByLocation.Count > 0)
            {
                return;
            }

            RebuildCaches();
        }

        private void RebuildCaches()
        {
            m_StartNodesByGuid = new Dictionary<string, StartNodeData>();
            m_PlayerNodesByGuid = new Dictionary<string, PlayerNodeData>();
            m_NpcNodesByGuid = new Dictionary<string, NpcNodeData>();
            m_StartNodesByLocation = new Dictionary<Location, List<StartNodeData>>();

            foreach (StartNodeData startNode in StartNodes)
            {
                startNode.HasPerformed = false;
                m_StartNodesByGuid[startNode.Guid] = startNode;

                Location location = GetStartNodeLocation(startNode);

                if (!m_StartNodesByLocation.TryGetValue(location, out List<StartNodeData> list))
                {
                    list = new List<StartNodeData>();
                    m_StartNodesByLocation.Add(location, list);
                }

                list.Add(startNode);
            }

            foreach (PlayerNodeData playerNode in PlayerNodes)
            {
                m_PlayerNodesByGuid[playerNode.Guid] = playerNode;
            }

            foreach (NpcNodeData npcNode in NpcNodes)
            {
                m_NpcNodesByGuid[npcNode.Guid] = npcNode;
            }

            if (m_StartNodesByLocation.Count < 1)
            {
                Debug.LogWarning("No Location Start Node data!!");
            }
        }

        private static Location GetStartNodeLocation(StartNodeData startNode)
        {
            if (startNode == null || startNode.Conditions == null)
            {
                return Location.Any;
            }

            return startNode.Conditions.Location;
        }

        public bool TryGetFreeStart(out StartNodeData startNode)
        {
            return TryGetFreeStart(Location.Any, out startNode);
        }

        public bool TryGetFreeStart(Location location, out StartNodeData startNode)
        {
            startNode = null;

            if (TryGetDefaultStartNode(location, out StartNodeData localDefault))
            {
                startNode = localDefault;
                return true;
            }

            if (location != Location.Any && TryGetDefaultStartNode(Location.Any, out StartNodeData anyDefault))
            {
                startNode = anyDefault;
                return true;
            }

            return false;
        }

        public bool TryGetDefaultStartNode(Location location, out StartNodeData startNode)
        {
            startNode = null;

            if (!TryGetStartNodesByLocation(location, out List<StartNodeData> startNodes))
            {
                return false;
            }

            foreach (var candidate in startNodes.Where(candidate => candidate != null).Where(candidate => candidate.Conditions == null || !candidate.Conditions.IsInitializedForDebug()))
            {
                startNode = candidate;
                return true;
            }

            return false;
        }

        public bool TryGetTriggeredStartNodes(Location location, Olay olay, out List<StartNodeData> startNodes)
        {
            startNodes = null;

            if (olay == null)
            {
                return false;
            }

            if (!TryGetStartNodesByLocation(location, out List<StartNodeData> locationStarts))
            {
                return false;
            }

            List<StartNodeData> matches = new();

            foreach (var startNode in locationStarts.Where(startNode => startNode != null && startNode.Conditions != null).Where(startNode => startNode.Conditions.IsInitializedForDebug()))
            {
                if (startNode.Conditions.TryGetMatchingOlays(olay, location, out List<Olay> matchingOlays) &&
                    matchingOlays is { Count: > 0 })
                {
                    matches.Add(startNode);
                }
            }

            if (matches.Count < 1)
            {
                return false;
            }

            startNodes = matches;
            return true;
        }

        public bool TryGetStartNodesByLocation(Location location, out List<StartNodeData> startNodes)
        {
            InitializeIfReq();
            return m_StartNodesByLocation.TryGetValue(location, out startNodes);
        }

        public bool TryGetStartNodeByGuid(string guid, out StartNodeData startNodeData)
        {
            InitializeIfReq();
            return m_StartNodesByGuid.TryGetValue(guid, out startNodeData);
        }

        public bool TryGetPlayerNodeDataByGuid(string guid, out PlayerNodeData playerNodeData)
        {
            InitializeIfReq();

            if (string.IsNullOrEmpty(guid))
            {
                playerNodeData = null;
                return false;
            }

            return m_PlayerNodesByGuid.TryGetValue(guid, out playerNodeData);
        }

        public bool TryGetNpcNodeDataByGuid(string guid, out NpcNodeData npcNodeData)
        {
            InitializeIfReq();
            return m_NpcNodesByGuid.TryGetValue(guid, out npcNodeData);
        }

        public bool IsNpcGuid(string guid)
        {
            InitializeIfReq();
            return !string.IsNullOrEmpty(guid) && m_NpcNodesByGuid.ContainsKey(guid);
        }

        public bool IsPlayerGuid(string guid)
        {
            InitializeIfReq();
            return !string.IsNullOrEmpty(guid) && m_PlayerNodesByGuid.ContainsKey(guid);
        }

        public bool TryGetNode(string guid, out NodeData node)
        {
            InitializeIfReq();

            if (m_PlayerNodesByGuid.TryGetValue(guid, out PlayerNodeData playerNodeData))
            {
                node = playerNodeData;
                return true;
            }

            if (m_NpcNodesByGuid.TryGetValue(guid, out NpcNodeData npcNodeData))
            {
                node = npcNodeData;
                return true;
            }

            if (m_StartNodesByGuid.TryGetValue(guid, out StartNodeData startNodeData))
            {
                node = startNodeData;
                return true;
            }

            node = null;
            return false;
        }

        public bool TryValidateStartNodes(out string errorMessage)
        {
            InitializeIfReq();

            errorMessage = string.Empty;

            foreach (var location in from pair in m_StartNodesByLocation let location = pair.Key let startNodes = pair.Value where startNodes != null && startNodes.Count != 0 let unconditionalCount = (from startNode in startNodes where startNode != null select startNode.Conditions != null && startNode.Conditions.IsInitializedForDebug()).Count(hasConditions => !hasConditions) where unconditionalCount > 1 select location)
            {
                errorMessage = $"Location '{location}' has more than one unconditional StartNode.";
                return false;
            }

            return true;
        }

#if UNITY_EDITOR
        public void SerializeDene()
        {
            DialogueJsonHandler.SaveDialogue(this);
        }

        public void DeSerializeDialogue()
        {
            DialogueJsonHandler.LoadDialogue(this.name);
        }
#endif
    }
}