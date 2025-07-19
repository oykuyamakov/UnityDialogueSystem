using System;
using System.Collections.Generic;
using DialogueManagement.Actions;
//using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace DialogueManagement.Data
{
    #if UNITY_EDITOR
    public abstract class DialogueContainerExtensions
    {
        [MenuItem("Assets/Create/Create Dialogue From Json")]
        public static void CreateCustomScript()
        {
            Object selectedAsset = Selection.activeObject;

            var path = AssetDatabase.GetAssetPath(selectedAsset);
            var dialogueContainer = DialogueJsonHandler.LoadDialogueFromPath(AssetDatabase.GetAssetPath(selectedAsset));
            
            Debug.Log(path);
            AssetDatabase.CreateAsset(dialogueContainer, path +"" + selectedAsset.name + ".asset");
            
            AssetDatabase.SaveAssets();
        }
    }
    #endif
    
    [Serializable]
    public class DialogueContainer : ScriptableObject
    {
        public NpcName NpcName = NpcName.Any;
        
        [SerializeField]
        //[ListDrawerSettings (DefaultExpandedState = true, ShowFoldout = false, HideAddButton = true, HideRemoveButton = true)]
        public List<NpcNodeData> NpcNodes = new();
        [SerializeField]
        public List<PlayerNodeData> PlayerNodes = new();
        [SerializeField]
        public List<StartNodeData> StartNodes = new();
        
        private Dictionary<string, PlayerNodeData> m_PlayerNodesByGuid = new();
        private Dictionary<string, NpcNodeData> m_NpcNodesByGuid = new();
        private Dictionary<string, StartNodeData> m_StartNodesByGuid = new();
        
        private Dictionary<Location, List<StartNodeData>> m_StartNodesByLocation = new();
        private Dictionary<Location,List<StartNodeData>> m_ConditionedStartNodesByLocation = new();
        
        public void ResettingKind()
        {
            m_StartNodesByGuid = new Dictionary<string, StartNodeData>();
            m_PlayerNodesByGuid = new Dictionary<string, PlayerNodeData>();
            m_NpcNodesByGuid = new Dictionary<string, NpcNodeData>();
            m_ConditionedStartNodesByLocation = new Dictionary<Location,List<StartNodeData>>();
            m_StartNodesByLocation = new Dictionary<Location, List<StartNodeData>>();

            foreach (var start in StartNodes)
            {
                start.HasPerformed = false;
            }
            
            InitializeIfReq();
        }
        
        public void InitializeIfReq()
        {
            if(m_StartNodesByGuid.Keys.Count < 1)
            {
                //Debug.Log("Start Dictionary Not Initialized, Initializing Now");
                m_StartNodesByGuid = new Dictionary<string, StartNodeData>();

                foreach (var t in StartNodes)
                {
                    m_StartNodesByGuid.Add(t.Guid, t);
                }
            }
            
            if (m_PlayerNodesByGuid.Keys.Count < 1)
            {
                //Debug.Log("Player Dictionary Not Initialized, Initializing Now");
                m_PlayerNodesByGuid = new Dictionary<string, PlayerNodeData>();

                foreach (var t in PlayerNodes)
                {
                    m_PlayerNodesByGuid.Add(t.Guid, t);
                }
            }
            
            if (m_NpcNodesByGuid.Keys.Count < 1)
            {
                //Debug.Log("Npc Dictionary Not Initialized, Initializing Now");
                m_NpcNodesByGuid = new Dictionary<string, NpcNodeData>();

                foreach (var t in NpcNodes)
                {
                    m_NpcNodesByGuid.Add(t.Guid, t);
                }
            }
            
            if (m_ConditionedStartNodesByLocation.Keys.Count < 1)
            {
                //Debug.Log("Condition Dic Not Initialized, Initializing Now");
                m_ConditionedStartNodesByLocation = new Dictionary<Location, List<StartNodeData>>();
            
                foreach (var t in StartNodes)
                {
                    t.HasPerformed = false;
                    
                    if (m_ConditionedStartNodesByLocation.TryGetValue(t.DialogueConditionContainer.Location, out var list))
                    {
                        if(list.Contains(t))
                        {
                            Debug.LogWarning("StartNodeData already exists in conditioned start nodes for location: " + t.DialogueConditionContainer.Location);
                        }
                        else
                        {
                            list.Add(t);
                        }
                    }
                    else
                    {
                        m_ConditionedStartNodesByLocation.Add(t.DialogueConditionContainer.Location, new List <StartNodeData>
                        {
                            t
                        });
                    }
                }
            }
            
            if (m_StartNodesByLocation.Keys.Count < 1)
            {
                m_StartNodesByLocation = new Dictionary<Location, List<StartNodeData>>();

                foreach (var t in StartNodes)
                {
                    if (m_StartNodesByLocation.ContainsKey(t.DialogueConditionContainer.Location))
                    {
                        m_StartNodesByLocation[t.DialogueConditionContainer.Location].Add(t);
                    }
                    else
                    {
                        m_StartNodesByLocation.Add(t.DialogueConditionContainer.Location, new List<StartNodeData> { t });
                    }
                }
                
                if (m_StartNodesByLocation.Keys.Count < 1)
                {
                    Debug.LogWarning("No Location Start Node data!!");
                }
            }
        }

        public bool TryGetFreeStart(out StartNodeData startNode)
        {
            startNode = null;
            
            if (!TryGetStartNodesByLocation(Location.Any, out var noLocStarts))
                return false;
            
            foreach (var sNode in noLocStarts)
            {
                if (sNode.DialogueConditionContainer.IsSatisfied())
                {
                    startNode = sNode;
                    return true;
                }
            }

            return false;
        }
        
        public bool TryGetStartNodesByLocation(Location location, out List<StartNodeData> startNodes)
        {
            InitializeIfReq();

            if (m_ConditionedStartNodesByLocation.TryGetValue(location, out startNodes))
            {
                return true;
            }
            
            //Debug.LogWarning("Condition start node not found" + location + " " + action + this.name);
            return false;
        }
        
        public bool TryGetStartNodeByGuid(string guid, out StartNodeData sNodeData)
        {
            InitializeIfReq();

            if (m_StartNodesByGuid.TryGetValue(guid, out sNodeData))
            {
                return true;
            }
            
            //Debug.LogError("StartNodeData is corrupted");
            return false;
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
            //Debug.LogError("PlayerNodeData is corrupted");
        }
        
        public bool TryGetNpcNodeDataByGuid(string guid, out NpcNodeData npcNodeData)
        {
            InitializeIfReq();
            
            if (m_NpcNodesByGuid.TryGetValue(guid, out npcNodeData))
            {
                return true;
            }
            
            //Debug.LogError("NpcNodeData is corrupted");
            return false;
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

            if (m_PlayerNodesByGuid.TryGetValue(guid, out var playerNodeData))
            {
                node = playerNodeData;
                return true;
            }

            if (m_NpcNodesByGuid.TryGetValue(guid, out var npcNodeData))
            {
                node = npcNodeData;
                return true;
            }

            if (m_StartNodesByGuid.TryGetValue(guid, out var startNodeData))
            {
                node = startNodeData;
                return true;
            }
            
            node = null;
            return false;
        }
        
#if UNITY_EDITOR
        
        //[Button]
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
