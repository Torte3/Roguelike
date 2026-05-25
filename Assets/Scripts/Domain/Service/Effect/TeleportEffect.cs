using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Setting;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class TeleportEffect : ActorlessEntityTargetEffect
    {
        public override Impact Impact => Impact.Neutral;
        public override Color Color => Colors.SkyBlue;

        public override async UniTask Apply(IEntity target, Vector2Int position, IMap map)
        {
            var randomPosition = map.GetAllBlankAndStandablePositionsOn(EntityLayer.Middle).GetAtRandom().Position;
            target.Entity.Teleport(randomPosition);
            await UniTask.Delay(Settings.GlobalSettings.MoveMilliseconds.CurrentValue);
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 0.1f;
        }

        public override float EvaluatePrice()
        {
            return 50f;
        }

        public override string Info()
        {
            return "対象をテレポートさせる\n";
        }
    }
}