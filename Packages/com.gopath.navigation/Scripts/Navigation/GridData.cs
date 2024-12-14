using UnityEngine;
using System.Collections.Generic;

namespace GoPath.Navigation
{
    [CreateAssetMenu(fileName = "GridData", menuName = "Navigation/Grid Data")]
    public class GridData : ScriptableObject
    {
        public float gridSpacing;
        public List<SerializableNode> nodes = new List<SerializableNode>();
        public Bounds mapBounds;

        [System.Serializable]
        public class SerializableNode
        {
            public Vector2 position;
            public List<int> connectionIndices = new List<int>();

            public SerializableNode(Vector2 pos)
            {
                position = pos;
            }
        }
    }
}
