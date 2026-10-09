using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface IEffectPosition
    {
        public bool IsDirectional { get; }
        public bool IsAtFeet => false;
        public Sprite ProjectileIcon => null;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map);

        public float EvaluateHitProbability();
        public string Description();
    }
}