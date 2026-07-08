using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class TeleportEffect : ActorlessEntityTargetEffect
    {
        public override Impact Impact => Impact.Neutral;
        public override Color Color => Colors.SkyBlue;

        public override UniTask Apply(IEntity target, Vector2Int position, IMap map)
        {
            var randomPosition = map.GetAllBlankAndStandablePositionsOn(EntityLayer.Middle).GetAtRandom().Position;
            target.Entity.Teleport(randomPosition);
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.1f;
        }

        public override float EvaluatePrice()
        {
            return 50f;
        }

        public override string Description()
        {
            return "対象をテレポートさせる\n";
        }
    }
}