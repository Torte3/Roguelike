using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Area
{
    public interface IArea
    {
        public bool IsDirectional => this is not INotDirectionalArea;
        public bool IsSelf => false;
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map);
        public float EvaluateArea();
        public string Description();
    }
}