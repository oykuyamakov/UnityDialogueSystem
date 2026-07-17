using System;
using UnityEngine;

namespace RealtVJ.Data
{
    [Serializable]
    public class BoxData : ISerializationCallbackReceiver
    {
        [HideInInspector]
        [SerializeField]
        public string Guid;

        [HideInInspector]
        [SerializeField]
        public float GraphX;

        [HideInInspector]
        [SerializeField]
        public float GraphY;

        [NonSerialized]
        public Vector2 GraphPosition;

        public BoxData() { }

        public BoxData(string guid, float graphX, float graphY)
        {
            Guid = guid;
            GraphX = graphX;
            GraphY = graphY;
            GraphPosition = new Vector2(graphX, graphY);
        }

        public BoxData(string guid, Vector2 position)
        {
            Guid = guid;
            GraphPosition = position;
            GraphX = position.x;
            GraphY = position.y;
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
