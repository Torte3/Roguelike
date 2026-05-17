using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AbsorbsEffect : EntityTargetEffect
    {
        [SerializeField] private List<ElementPower> _elementPowers;
        [Range(0, 1)][SerializeField] private float _rate;
        private float _fixedRate => Mathf.Clamp(_rate, 0, 1);
        [Range(0, 1)][SerializeField] private float _criticalRate;
        [SerializeField][HideInInspector] private bool _isWeaponAttack;
        private float _fixedCriticalRate => Mathf.Clamp(_criticalRate, 0, 1);

        public override Color Color => Colors.Yellow;

        public override Impact Impact => Impact.Harmful;

        public AbsorbsEffect(List<ElementPower> elementPowers, float rate, float criticalRate, bool isWeaponAttack = false)
        {
            _elementPowers = elementPowers;
            _rate = rate;
            _criticalRate = criticalRate;
            _isWeaponAttack = isWeaponAttack;
        }

        public void MultiplyPower(float multiplier)
        {
            foreach (var elementPower in _elementPowers)
            {
                elementPower.MultiplyPower(multiplier);
            }
        }

        public override async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            if (RandUtils.IsLessThanProbability(GetEffectiveCriticalRate(actor)))
            {
                var value = Formula.Calc(actor, target, _elementPowers, true);
                GameLog.AddAppend(target.IsVisible, $"<color=red>クリティカル！{target.GetName(map.Player)}に{value}のダメージ。</color>");
                var loseValue = await target.LoseHp(value, $"は{actor.GetName(map.Player)}の攻撃で殺された", actor as ICharacter);
                actor.GainHp(Mathf.RoundToInt(loseValue * _fixedRate * 2));
            }
            else
            {
                var value = Formula.Calc(actor, target, _elementPowers);
                GameLog.AddAppend(target.IsVisible, $"{target.GetName(map.Player)}に{value}のダメージ。");
                var loseValue = await target.LoseHp(value, $"は{actor.GetName(map.Player)}の攻撃で殺された", actor as ICharacter);
                actor.GainHp(Mathf.RoundToInt(loseValue * _fixedRate));
            }
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var criticalRate = GetEffectiveCriticalRate(actor);
            var result = Mathf.Min(1,
                             Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers)) /
                             target.CurrentMaxHp) *
                         (1 - criticalRate);
            result += Mathf.Min(1,
                Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers, true)) /
                target.CurrentMaxHp) * criticalRate;

            var normalDamage = Formula.Calc(actor, target, _elementPowers);
            var criticalDamage = Formula.Calc(actor, target, _elementPowers, true);
            var expectedDamage = normalDamage * (1 - criticalRate) + criticalDamage * criticalRate;
            var heal = expectedDamage * _fixedRate;

            var lostRatio = (float)(actor.CurrentMaxHp - actor.CurrentHp) / actor.CurrentMaxHp;
            var healRatio = heal / actor.CurrentMaxHp;
            if (lostRatio >= healRatio)
            {
                result += healRatio;
            }

            if (lostRatio > 0.5f)
            {
                result += lostRatio;
            }

            return result;
        }

        private float GetEffectiveCriticalRate(IActorOfEffect actor)
        {
            if (_isWeaponAttack
                && actor.Status.IsFlagStat(FlagStatType.FullHpCritical)
                && actor.CurrentHp >= actor.CurrentMaxHp)
                return 1f;

            return _fixedCriticalRate;
        }

        public override float EvaluatePrice()
        {
            return (Formula.EvaluateDamage(_elementPowers) * (1 - _fixedCriticalRate) +
                         Formula.EvaluateDamage(_elementPowers, true) * _fixedCriticalRate) * (1 + _fixedRate);
        }

        public override string Info()
        {
            var powers = string.Join("/", _elementPowers.Select(e => $"{e.Element.Name()}{e.Power}"));
            var info = $"攻撃[{ItemDescriptionRichText.RichAttackPowerSummary(powers)}]\n";
            if (_fixedCriticalRate > 0)
            {
                info += "そのとき" + ItemDescriptionRichText.ColorPercentagesInPlainText($"{_fixedCriticalRate:P0}") +
                        "の確率でクリティカルを発生させる\n";
            }

            info += "与ダメの" + ItemDescriptionRichText.ColorPercentagesInPlainText($"{_fixedRate:P0}") + "吸収\n";
            return info;
        }
    }
}