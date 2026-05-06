using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Logs;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AttackEffect : EntityTargetEffect
    {
        [RequiredListLength(1, null)]
        [SerializeField]
        private List<ElementPower> _elementPowers;

        [Range(0, 1)][SerializeField] private float _criticalRate;
        private float _fixedCriticalRate => Mathf.Clamp(_criticalRate, 0, 1);

        public AttackEffect(List<ElementPower> elementPowers, float criticalRate)
        {
            _elementPowers = elementPowers;
            _criticalRate = criticalRate;
        }

        public List<ElementPower> MultiplyPower(float multiplier)
        {
            var result = new List<ElementPower>();
            foreach (var elementPower in _elementPowers)
            {
                result.Add(elementPower.MultiplyPower(multiplier));
            }
            return result;
        }

        public override Color Color => Colors.Red;
        public override Impact Impact => Impact.Harmful;

        public override async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            if (RandUtils.IsLessThanProbability(_fixedCriticalRate))
            {
                var damage = Formula.Calc(actor, target, _elementPowers, true);
                GameLog.AddAppend(target.IsVisible, $"<color=red>クリティカル！{target.GetName(map.Player)}に{damage}のダメージ。</color>");
                await target.LoseHp(damage, $"は{actor.GetName(map.Player)}の攻撃で殺された", actor as ICharacter);
            }
            else
            {
                var damage = Formula.Calc(actor, target, _elementPowers);
                GameLog.AddAppend(target.IsVisible, $"{target.GetName(map.Player)}に{damage}のダメージ。");
                await target.LoseHp(damage, $"は{actor.GetName(map.Player)}の攻撃で殺された", actor as ICharacter);
            }
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var result = Mathf.Min(1,
                             Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers)) /
                             target.CurrentMaxHp) *
                         (1 - _fixedCriticalRate);
            result += Mathf.Min(1,
                Mathf.Min(target.CurrentHp, (float)Formula.Calc(actor, target, _elementPowers, true)) /
                target.CurrentMaxHp) * _fixedCriticalRate;
            return result;
        }

        public override float EvaluatePrice()
        {
            var result = Formula.EvaluateDamage(_elementPowers) * (1 - _fixedCriticalRate) +
                         Formula.EvaluateDamage(_elementPowers, true) * _fixedCriticalRate;
            return result;
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

            return info;
        }
    }
}