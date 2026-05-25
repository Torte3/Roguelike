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
using Utilities.Serialize;

namespace Domain.Service.Effect
{
    [Serializable]
    public class SpawnTrapEffect : ActorlessFieldTargetEffect
    {
        [Required][SerializeField] private ScriptableObjectSerializable<TrapData> _trap;
        [MinValue(1)][SerializeField] private int _count = 1;

        public override Color Color => Colors.DarkOrange;
        public override Impact Impact => Impact.Neutral;

        public override UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            return Apply(positions, map);
        }

        public override UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions = positions.Where(position => map.At(position).IsBlankAndStandable(EntityLayer.Floor)).ToList();
            var spawnCount = Mathf.Min(placeablePositions.Count, _count);
            foreach (var position in placeablePositions.GetAtRandom(spawnCount))
            {
                map.SpawnTrap(_trap.Value, position);
            }

            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            return 30f / CommonSenseParameters.MonsterMaxHealth;
        }

        public override float EvaluatePrice()
        {
            return 30f;
        }

        public override string Info()
        {
            return $"{_trap.Value.name}を{_count}個設置\n";
        }
    }
}
