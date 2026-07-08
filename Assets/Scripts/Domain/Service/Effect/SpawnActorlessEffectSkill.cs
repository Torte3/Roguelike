#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    public class SpawnActorlessEffectSkill : ISerializable<SpawnActorlessEffectSkillMemento>
    {
        private readonly IPositionOnlyDependentEffectPosition _position;
        private readonly INotDirectionalArea _area;
        private readonly List<IActorlessEffect> _effects;
        public int Repeats { get; private set; }
        public float ProbabilityOfSuccess { get; private set; }
        private readonly string? _log;

        public SpawnActorlessEffectSkill(SpawnActorlessEffectSkillMemento data)
        {
            _position = data.Position;
            _area = data.Area;
            _effects = data.Effects;
            Repeats = data.Repeats;
            ProbabilityOfSuccess = data.ProbabilityOfSuccess;
            _log = data.Log;
        }

        public Color Color => _effects.First().Color;
        public bool IsDirectional => _area.IsDirectional || _position.IsDirectional;
        public bool IsUsable() => true;

        public SpawnActorlessEffectSkillMemento Serialize()
        {
            return new SpawnActorlessEffectSkillMemento
            (
                _position,
                _area,
                _effects,
                Repeats,
                ProbabilityOfSuccess,
                _log
            );
        }

        public static SpawnActorlessEffectSkillMemento Build(IActorlessSkillData data)
        {
            return new SpawnActorlessEffectSkillMemento
            (
                data.Position,
                data.Area,
                data.Effects,
                data.Repeats,
                data.ProbabilityOfSuccess,
                data.Log
            );
        }

        private IEnumerable<Vector2Int> GetArea(Vector2Int position,
            IMap map)
        {
            var spawnPositions = _position.Get(position, map);

            return spawnPositions
                .SelectMany(spawnPosition => _area.Get(spawnPosition, map));
        }

        public async UniTask<ISkillResult> Use(Vector2Int position, IMap map, Id<IEntity>? excludeEntityId = null)
        {
            var successes = RandUtils.RollSuccesses(Repeats, ProbabilityOfSuccess);

            for (var i = 0; i < successes; i++)
            {
                var area = GetArea(position, map).ToList();
                if (_effects.Any(effect =>
                    effect is AttackEffect ||
                    effect is AbsorbsEffect ||
                    effect is PercentageDamageEffect ||
                    effect is BreakEffect))
                {
                    map.SetGrasses(area, false);
                }

                if (_effects.Any(effect =>
                    effect is AttackEffect ||
                    effect is AbsorbsEffect ||
                    effect is PercentageDamageEffect))
                {
                    map.AttackStatue(area);
                }
                var round = new SkillRound();
                foreach (var effect in _effects)
                {
                    foreach (var target in map.Entities.In(area)
                                 .OrderBy(target => Vector2.Distance(target.Entity.CurrentPosition, position))
                                 .Reverse())
                    {
                        if (excludeEntityId != null && target.Entity.Id == excludeEntityId)
                            continue;

                        switch (target)
                        {
                            case ICharacter character:
                                await effect.Apply(character, position, map);
                                round.Hit(character, effect);
                                break;
                            default:
                                await effect.Apply(target, position, map);
                                break;
                        }
                    }

                    await effect.Apply(area, map);
                }

                round.Record(map, area, Color);
            }

            if (successes == 0)
            {
                map.Events.Record(new SkillFailed(Visibility.At(map, position), SkillFailureKind.NoEffect, false));
                return SkillOutcome.Failed;
            }
            else if (successes < Repeats)
            {
                map.Events.Record(new SkillPartiallySucceeded(Visibility.At(map, position), successes, false));
            }

            return SkillOutcome.Success;
        }

        public float EvaluatePrice()
        {
            var price = 0f;
            foreach (var effect in _effects)
            {
                price += effect.EvaluatePrice();
            }
            price *= Repeats;

            price *= _area.EvaluateArea();
            price *= _position.EvaluateHitProbability();
            return price * ProbabilityOfSuccess;
        }

    }
}