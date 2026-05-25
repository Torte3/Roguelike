using System.Collections.Generic;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Area
{
    public class SelfArea : INotDirectionalArea
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position, IMap map)
        {
            return new List<Vector2Int> { position };
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map)
        {
            return Get(position, map);
        }

        public float EvaluateArea()
        {
            return 1;
        }

        public string Info()
        {
            return "その場";
        }
    }
}