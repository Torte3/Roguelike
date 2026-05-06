using System;
using System.Collections.Generic;
using System.Linq;
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
    public class TeleportToAreaEffect : FieldTargetEffect
    {
        public override Impact Impact => Impact.Neutral;
        public override Color Color => Colors.SkyBlue;

        public override UniTask Apply(IActorOfEffect actor, IEnumerable<Vector2Int> positions, IMap map)
        {
            var placeablePositions = positions.Where(pos =>
                map.At(pos).CanPlace(actor.IsFlying, actor.CanThroughWalls, false, EntityLayer.Middle));
            if (placeablePositions.Any())
            {
                actor.Entity.Teleport(placeablePositions.GetAtRandom());
            }
            else
            {
                actor.Entity.Teleport(map.FindBlankPositionFrom(positions.GetAtRandom(),
                    pos => map.At(pos).CanPlace(actor.IsFlying, actor.CanThroughWalls, false, EntityLayer.Middle)));
            }

            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            if (positions.Contains(actor.Entity.CurrentPosition))
                return 0;
            return 0.05f *
                   positions.Average(pos => VectorExtension.ChebyshevDistance(actor.Entity.CurrentPosition, pos));
        }

        public override float EvaluatePrice()
        {
            return 50f;
        }

        public override string Info()
        {
            return "対象に向かってテレポート\n";
        }
    }
}