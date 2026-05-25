using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface IActorlessEffectPosition : IEffectPosition
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map);
    }
    public interface IPositionOnlyDependentEffectPosition : IActorlessEffectPosition
    {
        public IEnumerable<Vector2Int> Get(Vector2Int position, IMap map);
    }
}