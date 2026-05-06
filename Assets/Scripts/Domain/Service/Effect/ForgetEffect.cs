using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class ForgetEffect : ActorlessEntityTargetEffect
    {
        public override Color Color => Colors.White;
        public override Impact Impact => Impact.Harmful;

        public override UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            target.ClearKnownItems(map);
            target.ClearAffiliation(map);

            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.2f;
        }

        public override float EvaluatePrice()
        {
            return 100;
        }

        public override string Info()
        {
            return "知識を消去\n";
        }
    }
}