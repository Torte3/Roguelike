#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect.Position;
using Domain.Model.Effect.Skill;
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

        public SpawnActorlessEffectSkill(SpawnActorlessEffectSkillMemento data)
        {
            _position = data.Position;
            _area = data.Area;
            _effects = data.Effects;
            Repeats = data.Repeats;
            ProbabilityOfSuccess = data.ProbabilityOfSuccess;
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
                ProbabilityOfSuccess
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
                data.ProbabilityOfSuccess
            );
        }

        private IEnumerable<Vector2Int> GetArea(Vector2Int position,
            IMap map)
        {
            var spawnPositions = _position.Get(position, map);

            return spawnPositions
                .SelectMany(spawnPosition => _area.Get(spawnPosition, map));
        }

        public async UniTask<ISkillResult> Use(Vector2Int position, IMap map, SkillSource source,
            Id<IEntity>? excludeEntityId = null)
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
                var hits = new AreaEffectHits();
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
                                hits.Add(character, effect);
                                break;
                            default:
                                await effect.Apply(target, position, map);
                                break;
                        }
                    }

                    await effect.Apply(area, map);
                }

                hits.Record(map, area, Color);
            }

            if (successes == 0)
            {
                map.Events.Record(new SkillFailed(map.PlayerCharacter.IsVisible(position), source, SkillFailureKind.NoEffect));
                return SkillOutcome.Failed;
            }
            else if (successes < Repeats)
            {
                map.Events.Record(new SkillPartiallySucceeded(map.PlayerCharacter.IsVisible(position), source, successes));
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