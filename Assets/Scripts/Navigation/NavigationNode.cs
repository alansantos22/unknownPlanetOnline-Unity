using UnityEngine;
using System.Collections.Generic;

namespace UnknownPlanet.Navigation
{
    public class NavigationNode
    {
        public Vector2 WorldPosition { get; private set; }
        public List<NavigationNode> Connections { get; private set; }
        public float GCost { get; set; }
        public float HCost { get; set; }
        public float FCost => GCost + HCost;
        public NavigationNode Parent { get; set; }

        public NavigationNode(Vector2 position)
        {
            WorldPosition = position;
            Connections = new List<NavigationNode>();
            GCost = float.MaxValue;
            HCost = 0;
            Parent = null;
        }

        public void AddConnection(NavigationNode other)
        {
            if (!Connections.Contains(other))
            {
                Connections.Add(other);
                other.Connections.Add(this); // Conexão bidirecional
            }
        }

        public void Reset()
        {
            GCost = float.MaxValue;
            HCost = 0;
            Parent = null;
        }
    }
}
