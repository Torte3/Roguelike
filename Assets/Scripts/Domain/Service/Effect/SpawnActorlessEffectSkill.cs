#nullable enable
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Effect;
using Domain.Model.Effect.Area;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    public class SpawnActorlessEffectSkill : ISerializable<SpawnActorlessEffectSkillMemento>, ISkill
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

        public IEnumerable<Vector2Int> GetArea(Vector2Int position,
            IMap map)
        {
            var spawnPositions = _position.Get(position, map);

            return spawnPositions
                .SelectMany(spawnPosition => _area.Get(spawnPosition, map));
        }

        public async UniTask<ISkillResult> Use(string? name, Vector2Int position, IMap map, Id<IEntity>? excludeEntityId = null)
        {
            if (_log != null && _log != "")
                GameLog.Add(map.Player.Character.IsVisible(position), $"{name}{_log}");

            var successes = RandUtils.RollSuccesses(Repeats, ProbabilityOfSuccess);

            for (var i = 0; i < successes; i++)
            {
                var area = GetArea(position, map);
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
                                break;
                            default:
                                await effect.Apply(target, position, map);
                                break;
                        }
                    }

                    await effect.Apply(area, map);
                }

                if (map.Player.Character.VisibleArea.Intersect(area).Any())
                {
                    map.SpawnEffect(area, Color);
                    await UniTask.Delay(Settings.GlobalSettings.EffectDisplayTime.CurrentValue);
                }
            }

            if (successes == 0)
            {
                GameLog.Add(map.Player.Character.IsVisible(position), "しかし効果がなかった");
                return SpawnEffectSkillResult.Failed;
            }
            else if (successes < Repeats)
            {
                GameLog.Add(map.Player.Character.IsVisible(position), $"{successes}回成功した");
            }

            return SpawnEffectSkillResult.Success;
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

        public string InfoOnUse(bool omitProbabilityOfSuccess = false, bool useOrThrowCombinedTargets = false)
        {
            var info = "";
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
            return info;
        }

        public string InfoOnThrow(bool omitEffects = false)
        {
            var info = "";
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
            return info;
        }
    }
}