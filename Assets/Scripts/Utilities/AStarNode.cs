using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    internal class AStarNode
    {
        private readonly Vector2Int _position;

        private AStarNode _rootNode;

        public float ECost;

        public float MoveTotalCost;

        public AStarNode(Vector2Int position, Vector2Int goal)
        {
            _position = position;
            SetEstimateCost(position, goal);
        }

        public float Score => ECost + MoveTotalCost;

        public void SetEstimateCost(Vector2Int position, Vector2Int goal)
        {
            ECost = VectorExtension.ChebyshevDistance(position, goal);
        }

        public void Open(AStarNode rootNode, float cost)
        {
            _rootNode = rootNode;
            if (_rootNode == null)
                MoveTotalCost = 0;
            else
                MoveTotalCost = _rootNode.MoveTotalCost + cost;
        }

        public List<Vector2Int> ToList()
        {
            var list = new List<Vector2Int>();
            list.Insert(0, _position);

            var node = _rootNode;
            while (node != null)
            {
                list.Insert(0, node._position);
                node = node._rootNode;
            }

            return list;
        }
    }
}