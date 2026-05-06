using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Logs;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class PercentageDamageEffect : ActorlessEntityTargetEffect
    {
        [SerializeField][Range(0, 1)] private float _damageRate = 0.5f;

        public PercentageDamageEffect(float damageRate)
        {
            _damageRate = damageRate;
        }

        public override Color Color => Colors.Red;

        public override Impact Impact => Impact.Harmful;

        public override async UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            var damage = Formula.CalcExplosionDamage(_damageRate, target);
            GameLog.AddAppend(target.IsVisible, $"{target.GetName(map.Player)}に{damage}のダメージ。");
            await target.LoseHp(damage, $"は爆発に巻き込まれた", null);
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            var result = Mathf.Min(1,
                Mathf.Min(target.CurrentHp, (float)Formula.CalcExplosionDamage(_damageRate, target)) /
                target.CurrentMaxHp);
            return result;
        }

        public override float EvaluatePrice()
        {
            return Formula.EvaluateExplosionDamage(_damageRate);
        }

        public override string Info()
        {
            var rate = $"割合{_damageRate:P0}";
            return $"攻撃[{ItemDescriptionRichText.RichAttackPowerSummary(rate)}]\n";
        }
    }
}