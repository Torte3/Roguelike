using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect.Position
{
    public class AllCharacter : IPositionOnlyDependentEffectPosition
    {
        public bool IsDirectional => false;

        public IEnumerable<Vector2Int> Get(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map)
        {
            return Get(position, map);
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, Direction8 direction, IMap map)
        {
            return Get(position, map);
        }

        public IEnumerable<Vector2Int> Get(Vector2Int position, IMap map)
        {
            return map.AllCharacterPositionsFast();
        }

        public float EvaluateHitProbability()
        {
            return 300;
        }

        public string Info()
        {
            return "すべてのキャラクターの場所";
        }
    }
}