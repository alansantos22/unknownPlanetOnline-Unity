using UnityEngine;
using System.Collections.Generic;

namespace GoPath.Navigation
{
    public class NavigationNode
    {
        public Vector2 WorldPosition { get; private set; }
        public List<NavigationNode> Connections { get; private set; }
        public NavigationNode Parent { get; set; }
        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost => GCost + HCost;

        public NavigationNode(Vector2 position)
        {
            WorldPosition = position;
            Connections = new List<NavigationNode>();
            Reset();
        }

        public void Reset()
        {
            Parent = null;
            GCost = float.MaxValue;
            HCost = 0;
        }

        public void AddConnection(NavigationNode node)
        {
            if (!Connections.Contains(node))
            {
                Connections.Add(node);
            }
        }
    }
}
