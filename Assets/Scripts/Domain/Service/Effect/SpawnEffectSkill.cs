#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Affiliation;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Skill;
using Domain.Model.Entity;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Item.Description;
using Domain.Model.Map;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    public class SpawnEffectSkill : ISerializable<SpawnEffectSkillMemento>, ISkill
    {
        private readonly IEffectPosition _position;
        private readonly IArea _area;
        private readonly List<IEffect> _effects;
        public int Repeats { get; private set; }
        public float ProbabilityOfSuccess { get; private set; }
        private readonly string? _log;
        public string Log => _log ?? "";

        public SpawnEffectSkill(SpawnEffectSkillMemento data)
        {
            _position = data.Position;
            _area = data.Area;
            _effects = data.Effects;
            Repeats = data.Repeats;
            ProbabilityOfSuccess = data.ProbabilityOfSuccess;
            RushDistance = data.RushDistance;
            BackStepDistance = data.BackStepDistance;
            _log = data.Log;
        }

        public Color Color => _effects.First().Color;
        public bool IsDirectional => _area.IsDirectional || _position.IsDirectional;

        public int RushDistance { get; private set; }
        public int BackStepDistance { get; private set; }

        internal IEffectPosition EffectPosition => _position;

        internal IArea EffectArea => _area;

        internal IReadOnlyList<IEffect> EffectList => _effects;

        public SpawnEffectSkillMemento Serialize()
        {
            return new SpawnEffectSkillMemento
            (
                _position,
                _area,
                _effects,
                Repeats,
                ProbabilityOfSuccess,
                RushDistance,
                BackStepDistance,
                _log
            );
        }

        public static SpawnEffectSkillMemento Build(ISkillData data)
        {
            return new SpawnEffectSkillMemento
            (
                data.Position,
                data.Area,
                data.Effects,
                data.Repeats,
                data.ProbabilityOfSuccess,
                data.RushDistance,
                data.BackStepDistance,
                data.Log
            );
        }

        public IEnumerable<Vector2Int> GetArea(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map, bool onlyVisible)
        {
            position = AdvanceByRushDistance(actor, position, direction, map);
            return GetAreaIgnoreRush(actor, position, direction, map, onlyVisible);
        }

        private Vector2Int AdvanceByRushDistance(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map)
        {
            for (var i = 0; i < RushDistance; i++)
            {
                if (actor.CanMove(position, direction, map) && !actor.Status.IsFlagStat(FlagStatType.CannotMove))
                    position += direction.Vector();
                else
                    break;
            }

            return position;
        }

        private IEnumerable<Vector2Int> GetAreaIgnoreRush(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map, bool onlyVisible)
        {
            var spawnPositions = _position.Get(actor, position, direction, map);
            if (onlyVisible)
            {
                var visibleArea = actor.VisibleArea.ToHashSet();
                return AreaAround(spawnPositions.Where(visibleArea.Contains), direction, map)
                    .Where(visibleArea.Contains);
            }

            return AreaAround(spawnPositions, direction, map);
        }

        private IEnumerable<Vector2Int> AreaAround(IEnumerable<Vector2Int> spawnPositions, Direction8 direction, IMap map)
        {
            return spawnPositions.SelectMany(spawnPosition => _area.Get(spawnPosition, direction, map));
        }
        public async UniTask<ISkillResult> Use(IActorOfEffect actor, IItem? sourceItem, Vector2Int position,
            Direction8 direction, IMap map, SkillSource source)
        {
            var effectiveRepeats = GetEffectiveRepeats(actor, sourceItem);
            var successes = RandUtils.RollSuccesses(effectiveRepeats, ProbabilityOfSuccess);

            for (var i = 0; i < successes; i++)
            {
                var spawnPositions = _position.Get(actor, position, direction, map).ToList();
                if (_position is ProjectileImpact projectileImpact)
                {
                    var destination = projectileImpact.LandingOf(spawnPositions);
                    map.Events.Record(new ProjectileFlew(
                        map.PlayerCharacter.IsVisible(position) || map.PlayerCharacter.IsVisible(destination),
                        new Flight(projectileImpact.Icon.Value, position, destination)));
                }

                var area = AreaAround(spawnPositions, direction, map).ToList();
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
                    map.RevealMimic(area);
                    map.AttackStatue(area);
                }
                var hits = new AreaEffectHits();
                foreach (var effect in _effects)
                {
                    foreach (var target in map.Entities.In(area)
                                 .OrderBy(target => Vector2.Distance(target.Entity.CurrentPosition, position))
                                 .Reverse())
                    {
                        await ApplyEffectToTarget(effect, actor, target, position, map, hits);
                    }

                    await effect.Apply(actor, area, map);
                }

                hits.Record(map, area, Color);
            }

            if (successes == 0)
            {
                map.Events.Record(new SkillFailed(actor.Entity.IsVisible, source, SkillFailureKind.NoEffect));
                return SkillOutcome.Failed;
            }
            else if (successes < effectiveRepeats)
            {
                map.Events.Record(new SkillPartiallySucceeded(actor.Entity.IsVisible, source, successes));
            }

            return SkillOutcome.Success;
        }

        private async UniTask ApplyEffectToTarget(IEffect effect, IActorOfEffect actor, IEntity target,
            Vector2Int position, IMap map, AreaEffectHits hits)
        {
            switch (target)
            {
                case ICharacter character:
                    await effect.Apply(actor, character, position, map);
                    hits.Add(character, effect);

                    if (effect.Impact == Impact.Harmful)
                    {
                        var impactValue = effect.Evaluate(actor, character);
                        character.OnAttackedBy(actor, impactValue);
                        ReflectAttackToNearbyAffiliations(map, actor, character, impactValue);
                    }
                    else if (effect.Impact == Impact.Beneficial)
                    {
                        var impactValue = effect.Evaluate(actor, character);
                        character.OnHealedBy(actor, impactValue);
                        ReflectHealToNearbyAffiliations(map, actor, character, impactValue);
                    }

                    break;
                default:
                    await effect.Apply(actor, target, position, map);
                    break;
            }
        }

        private void ReflectAttackToNearbyAffiliations(IMap map, IActorOfEffect actor, ICharacter target,
            float impactValue)
        {
            foreach (var witness in WitnessesOf(map, target, actor))
                witness.Affiliation.OnCharacterAttacked(actor.Affiliation, target.Affiliation, impactValue);
        }

        private void ReflectHealToNearbyAffiliations(IMap map, IActorOfEffect actor, ICharacter target,
            float impactValue)
        {
            foreach (var witness in WitnessesOf(map, target, actor))
                witness.Affiliation.OnCharacterHealed(actor.Affiliation, target.Affiliation, impactValue);
        }

        private IEnumerable<ICharacter> WitnessesOf(IMap map, ICharacter target, IActorOfEffect actor)
            => map.Characters.Where(witness => witness != actor && witness.SeesDeed(actor, target.Entity.CurrentPosition));

        public float Evaluate(IActorOfEffect actor, IItem? sourceItem, Vector2Int position, Direction8 direction,
            IMap map)
        {
            position = AdvanceByRushDistance(actor, position, direction, map);

            var area = GetArea(actor, position, direction, map, true);
            var characters = map.Characters.In(area);
            if (_effects.Any(ContainsEntityTargetEffect) && !characters.Any())
                return -1;

            var totalEvaluation = 0f;

            foreach (var effect in _effects)
            {
                foreach (var target in characters)
                {
                    switch (effect.Impact)
                    {
                        case Impact.Harmful:
                            var affiliationType = actor.Affiliation.GetAffiliationType(target.Affiliation);
                            totalEvaluation += actor.Aggression.GetAggression(affiliationType) *
                                               effect.Evaluate(actor, target);
                            break;
                        case Impact.Beneficial:
                            if (actor.IsAlly(target))
                            {
                                totalEvaluation += 1 * effect.Evaluate(actor, target);
                            }
                            else if (actor.IsEnemy(target))
                            {
                                totalEvaluation += -Mathf.Infinity * effect.Evaluate(actor, target);
                            }
                            else
                            {
                                totalEvaluation += 0 * effect.Evaluate(actor, target);
                            }

                            break;
                    }
                }

                totalEvaluation += effect.Evaluate(actor, area);
            }

            return totalEvaluation * GetEffectiveRepeats(actor, sourceItem) * ProbabilityOfSuccess;
        }

        private static bool ContainsEntityTargetEffect(IEffect effect) =>
            effect switch
            {
                EntityTargetEffect => true,
                ActorlessEntityTargetEffect => true,
                RandomEffect random => random.Effects.Any(ContainsEntityTargetEffect),
                _ => false
            };

        private int GetEffectiveRepeats(IActorOfEffect actor, IItem? sourceItem)
        {
            var repeats = Repeats;
            if (sourceItem?.Category == ItemCategory.Potions
                && actor.Status.IsFlagStat(FlagStatType.PotionMaster))
                repeats += CommonSenseParameters.PotionMasterEffectRepeatBonus;

            return repeats;
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

        public string Description()
        {
            return DescriptionOnUse();
        }

        internal string DescriptionOnUse(bool omitProbabilityOfSuccess = false, bool useOrThrowCombinedTargets = false)
        {
            var description = "";
            if (RushDistance > 0)
                description += $"最初に{ItemDescriptionRichText.RichSpatialCells(RushDistance)}前に進む\n";
            description += EffectTargetDescription.OnUse(_position, _area, useOrThrowCombinedTargets) + "\n";
            foreach (var effect in _effects)
            {
                description += effect.Description();
            }
            if (Repeats > 1)
                description += $"効果は{ItemDescriptionRichText.RichMeta(Repeats)}回発動する\n";
            if (!omitProbabilityOfSuccess)
                description += ItemDescriptionRichText.ColorPercentagesInPlainText($"成功率：{ProbabilityOfSuccess:P0}\n");

            if (BackStepDistance > 0)
                description += $"最後に{ItemDescriptionRichText.RichSpatialCells(BackStepDistance)}後ろに下がる\n";
            return description;
        }

        internal string DescriptionOnThrow(bool omitEffects = false)
        {
            var description = "";
            if (RushDistance > 0)
                description += $"最初に{ItemDescriptionRichText.RichSpatialCells(RushDistance)}前に進む\n";
            var targetLine = EffectTargetDescription.OnThrow(_position, _area);
            if (!omitEffects)
            {
                description += targetLine + "\n";
                foreach (var effect in _effects)
                {
                    description += effect.Description();
                }
            }
            else
            {
                description += targetLine + "\n";
                description += "使用時と同じ効果を発揮する\n";
            }

            if (Repeats > 1)
                description += $"効果は{ItemDescriptionRichText.RichMeta(Repeats)}回発動する\n";

            description += ItemDescriptionRichText.ColorPercentagesInPlainText($"成功率：{ProbabilityOfSuccess:P0}\n");

            if (BackStepDistance > 0)
                description += $"最後に{ItemDescriptionRichText.RichSpatialCells(BackStepDistance)}後ろに下がる\n";
            return description;
        }
    }
}