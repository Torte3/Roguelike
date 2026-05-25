using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;
using Utilities.Serialize;

namespace Domain.Service.Effect
{
    [Serializable]
    public class SpawnCharacterEffect : ActorlessFieldTargetEffect
    {
        [Required][SerializeField] private ScriptableObjectSerializable<EnemyData> _character;
        [MinValue(1)][SerializeField] private int _count;
        [SerializeField] private bool _inheritsShiny;

        public override Color Color => Colors.MediumPurple;
        public override Impact Impact => Impact.Neutral;

        public override UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions = positions.Where(position => map.At(position).CanPlace(_character.Value.IsFlying,
                _character.Value.CanThroughWalls, false, EntityLayer.Middle));

            var canSpawnCount = Mathf.Min(placeablePositions.Count(), _count);
            foreach (var position in placeablePositions.GetAtRandom(canSpawnCount))
            {
                map.SpawnEnemyIgnoreMimic(
                    _character.Value,
                    position,
                    doActImmediately: true,
                    affiliation: actor.Affiliation,
                    isSlept: false,
                    isShiny: _inheritsShiny ? actor.IsShiny : null
                );
            }

            return UniTask.CompletedTask;
        }

        public override UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions = positions.Where(position => map.At(position).CanPlace(_character.Value.IsFlying,
                _character.Value.CanThroughWalls, false, EntityLayer.Middle));
            if (placeablePositions.Any())
            {
                foreach (var position in placeablePositions.GetAtRandom(_count))
                {
                    map.SpawnEnemyIgnoreMimic(
                        _character.Value,
                        position,
                        doActImmediately: true
                    );
                }
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
            return $"{_character.Value.Name}を{_count}体召喚\n";
        }
    }
}