using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class AlertEffect : ActorlessEntityTargetEffect
    {
        public override Color Color => Colors.Red;

        public override Impact Impact => Impact.Neutral;

        public override UniTask Apply(ITargetOfEffect target, Vector2Int position, IMap map)
        {
            target.ListenToAlert(new Location(map.Id, position));

            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.2f;
        }

        public override float EvaluatePrice()
        {
            return 20;
        }

        public override string Info()
        {
            return "警報を鳴らす\n";
        }
    }
}