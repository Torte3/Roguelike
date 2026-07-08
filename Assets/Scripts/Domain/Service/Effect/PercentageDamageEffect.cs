using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item.Description;
using Domain.Model.Map;
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

        public override UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            return Damage(target, Opponent.Of(actor));
        }

        public override UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            return Damage(target, null);
        }

        private async UniTask Damage(ITargetOfEffect target, Opponent opponent)
        {
            var damage = Formula.CalcExplosionDamage(_damageRate, target);
            await target.LoseHp(damage, new PlainDamage(DamageCause.Explosion, opponent), null);
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

        public override string Description()
        {
            var rate = $"割合{_damageRate:P0}";
            return $"攻撃[{ItemDescriptionRichText.RichAttackPowerSummary(rate)}]\n";
        }
    }
}