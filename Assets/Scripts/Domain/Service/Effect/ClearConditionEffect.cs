using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    public class ClearConditionEffect : ActorlessEntityTargetEffect
    {
        public override Color Color => Colors.LightSkyBlue;
        public override Impact Impact => Impact.Beneficial;

        public override UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            target.ClearCondition();
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0;
        }

        public override float EvaluatePrice()
        {
            return 500;
        }

        public override string Info()
        {
            return "全状態異常を解除\n";
        }
    }
}