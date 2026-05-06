using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class SpawnRandomCharacterEffect : ActorlessFieldTargetEffect
    {
        [MinValue(1)] [SerializeField] private int _count;

        public override Color Color => Colors.MediumPurple;
        public override Impact Impact => Impact.Neutral;

        public override UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions =
                positions.Where(position => map.At(position).CanPlace(false, false, false, EntityLayer.Middle));
            var canSpawnCount = Mathf.Min(placeablePositions.Count(), _count);
            foreach (var position in placeablePositions.GetAtRandom(canSpawnCount))
            {
                map.SpawnRandomEnemy(
                    position,
                    isSlept: false
                );
            }

            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            return 50f / CommonSenseParameters.MonsterMaxHealth;
        }

        public override float EvaluatePrice()
        {
            return 50f;
        }

        public override string Info()
        {
            return $"ランダムに{_count}体召喚\n";
        }
    }
}