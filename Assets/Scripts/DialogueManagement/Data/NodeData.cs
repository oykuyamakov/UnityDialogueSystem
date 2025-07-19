using Newtonsoft.Json;
using UnityEngine;

namespace DialogueManagement.Data
{
    
    [System.Serializable]
    public class NodeData : ISerializationCallbackReceiver 
    {
        [HideInInspector][SerializeField]
        public string Guid;
        [HideInInspector][SerializeField][JsonIgnore]
        public Vector2 GraphPosition;

        [SerializeField][HideInInspector]
        public float GraphX;
        [SerializeField][HideInInspector]
        public float GraphY;
        
        
        [JsonConstructor]
        public NodeData(string guid, float graphX, float graphY)
        {
            Guid = guid;
            GraphPosition = new Vector2(graphX, graphY);
            GraphX = graphX;
            GraphY = graphY;
        }
        
        public NodeData(string guid, Vector2 graphPosition)
        {
            Guid = guid;
            GraphPosition = graphPosition;
            GraphX = graphPosition.x;
            GraphY = graphPosition.y;
        }
        
        public void OnBeforeSerialize()
        {
            GraphX = GraphPosition.x;
            GraphY = GraphPosition.y;
        }

        public void OnAfterDeserialize()
        {
            GraphPosition = new Vector2(GraphX, GraphY);
        }
    }
}
