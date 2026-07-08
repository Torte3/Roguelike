using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public interface IEffectPosition
    {
        public bool IsDirectional { get; }

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map);

        public float EvaluateHitProbability();
        public string Description();
    }
}