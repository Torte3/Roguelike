using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AffectionIncreaseEffect : EntityTargetEffect
    {
        [MinValue(1)] [SerializeField] private float _power;

        public override Color Color => Colors.HotPink;

        public override Impact Impact => Impact.Beneficial;

        public override UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return _power;
        }

        public override float EvaluatePrice()
        {
            return 100;
        }

        public override string Info()
        {
            return $"好感度を{_power}上昇させる\n";
        }
    }
}