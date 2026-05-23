#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Item;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Logs;
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

        public IEffectPosition EffectPosition => _position;

        public IArea EffectArea => _area;

        public IReadOnlyList<IEffect> EffectList => _effects;

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
            IMap map, bool onlyVisible = false)
        {
            for (var i = 0; i < RushDistance; i++)
            {
                if (actor.CanMove(position, direction, map) && !actor.Status.IsFlagStat(FlagStatType.CannotMove))
                    position += direction.Vector();
                else
                    break;
            }

            return GetAreaIgnoreRush(actor, position, direction, map, onlyVisible);
        }

        private IEnumerable<Vector2Int> GetAreaIgnoreRush(IActorOfEffect actor, Vector2Int position, Direction8 direction,
            IMap map, bool onlyVisible = false)
        {
            var spawnPositions = _position.Get(actor, position, direction, map);
            if (onlyVisible)
            {
                return spawnPositions
                    .Where(map.Player.Character.VisibleArea.Contains)
                    .SelectMany(spawnPosition => _area.Get(spawnPosition, direction, map))
                    .Where(map.Player.Character.VisibleArea.Contains);
            }

            return spawnPositions
                .SelectMany(spawnPosition => _area.Get(spawnPosition, direction, map));
        }
        public async UniTask<ISkillResult> Use(IActorOfEffect actor, IItem? sourceItem, Vector2Int position,
            Direction8 direction, IMap map)
        {
            if (_log != null && _log != "")
                GameLog.Add(actor.IsVisible, $"{actor.GetName(map.Player)}{_log}");

            var effectiveRepeats = GetEffectiveRepeats(actor, sourceItem);
            var successes = RandUtils.RollSuccesses(effectiveRepeats, ProbabilityOfSuccess);

            for (var i = 0; i < successes; i++)
            {
                if (_position is ProjectileImpact projectileImpact)
                {
                    await map.ShowThrowAnimation(projectileImpact.Icon.Value, position, direction,
                        CommonSenseParameters.ThrowDistance, projectileImpact.IsPiercing, projectileImpact.CanHitLayer.ToArray());
                }

                var area = GetAreaIgnoreRush(actor, position, direction, map);
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
                foreach (var effect in _effects)
                {
                    foreach (var target in map.Entities.In(area)
                                 .OrderBy(target => Vector2.Distance(target.Entity.CurrentPosition, position))
                                 .Reverse())
                    {
                        switch (target)
                        {
                            case ICharacter character:
                                await effect.Apply(actor, character, position, map);

                                if (effect.Impact == Impact.Harmful)
                                {
                                    var impactValue = effect.Evaluate(actor, character);
                                    character.OnAttackedBy(actor, impactValue);

                                    foreach (var c in map.GetCharactersCanSeePosition(character.Entity.CurrentPosition)
                                                            .Where(target => target != actor && target != actor))
                                    {
                                        c.Affiliation.OnCharacterAttacked(actor.Affiliation, character.Affiliation,
                                            impactValue);
                                    }

                                }
                                else if (effect.Impact == Impact.Beneficial)
                                {
                                    var impactValue = effect.Evaluate(actor, character);
                                    character.OnHealedBy(actor, impactValue);

                                    foreach (var c in map.GetCharactersCanSeePosition(character.Entity.CurrentPosition)
                                                            .Where(target => target != actor && target != actor))
                                    {
                                        c.Affiliation.OnCharacterHealed(actor.Affiliation, character.Affiliation,
                                            impactValue);
                                    }
                                }

                                break;
                            default:
                                await effect.Apply(actor, target, position, map);
                                break;
                        }
                    }

                    await effect.Apply(actor, area, map);
                }

                if (map.Player.Character.VisibleArea.Intersect(area).Any())
                {
                    map.SpawnEffect(area, Color);
                    await UniTask.Delay(Settings.GlobalSettings.EffectDisplayTime.CurrentValue);
                }
            }

            if (successes == 0)
            {
                GameLog.AddAppend(actor.IsVisible, "しかし効果がなかった");
                return SpawnEffectSkillResult.Failed;
            }
            else if (successes < effectiveRepeats)
            {
                GameLog.AddAppend(actor.IsVisible, $"{successes}回成功した");
            }

            return SpawnEffectSkillResult.Success;
        }

        public float Evaluate(IActorOfEffect actor, IItem? sourceItem, Vector2Int position, Direction8 direction,
            IMap map)
        {
            for (var i = 0; i < RushDistance; i++)
            {
                if (actor.CanMove(position, direction, map) && !actor.Status.IsFlagStat(FlagStatType.CannotMove))
                    position += direction.Vector();
            }

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

        public string Info()
        {
            return InfoOnUse();
        }

        public string InfoOnUse(bool omitProbabilityOfSuccess = false, bool useOrThrowCombinedTargets = false)
        {
            var info = "";
            if (RushDistance > 0)
                info += $"最初に{ItemDescriptionRichText.RichSpatialCells(RushDistance)}前に進む\n";
            var positionInfo = _position.Info();
            var areaInfo = _area.Info();
            info += EffectTargetDescription.OnUse(positionInfo, areaInfo, useOrThrowCombinedTargets) + "\n";
            foreach (var (effect, index) in _effects.Index())
            {
                info += ItemDescriptionRichText.StyleEffectInfo(effect, effect.Info());
            }
            if (Repeats > 1)
                info += $"効果は{ItemDescriptionRichText.RichMeta(Repeats)}回発動する\n";
            if (!omitProbabilityOfSuccess)
                info += ItemDescriptionRichText.ColorPercentagesInPlainText($"成功率：{ProbabilityOfSuccess:P0}\n");

            if (BackStepDistance > 0)
                info += $"最後に{ItemDescriptionRichText.RichSpatialCells(BackStepDistance)}後ろに下がる\n";
            return info;
        }

        public string InfoOnThrow(bool omitEffects = false)
        {
            var info = "";
            if (RushDistance > 0)
                info += $"最初に{ItemDescriptionRichText.RichSpatialCells(RushDistance)}前に進む\n";
            var positionInfo = _position.Info();
            var areaInfo = _area.Info();
            var targetLine = EffectTargetDescription.OnThrow(positionInfo, areaInfo);
            if (!omitEffects)
            {
                info += targetLine + "\n";
                foreach (var (effect, index) in _effects.Index())
                {
                    info += ItemDescriptionRichText.StyleEffectInfo(effect, effect.Info());
                }
            }
            else
            {
                info += targetLine + "\n";
                info += "使用時と同じ効果を発揮する\n";
            }

            if (Repeats > 1)
                info += $"効果は{ItemDescriptionRichText.RichMeta(Repeats)}回発動する\n";

            info += ItemDescriptionRichText.ColorPercentagesInPlainText($"成功率：{ProbabilityOfSuccess:P0}\n");

            if (BackStepDistance > 0)
                info += $"最後に{ItemDescriptionRichText.RichSpatialCells(BackStepDistance)}後ろに下がる\n";
            return info;
        }
    }
}