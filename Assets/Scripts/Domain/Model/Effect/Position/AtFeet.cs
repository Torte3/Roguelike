using System.Collections.Generic;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class AtFeet : IPositionOnlyDependentEffectPosition
    {
        public bool IsDirectional => false;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map) => Get(position, map);
        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map) => Get(position, map);
        public IEnumerable<Vector2Int> Get(Vector2Int position, IMap map)
        {
            yield return position;
        }

        public float EvaluateHitProbability()
        {
            return 1;
        }

        public string Info()
        {
            return "発動場所";
        }
    }
}