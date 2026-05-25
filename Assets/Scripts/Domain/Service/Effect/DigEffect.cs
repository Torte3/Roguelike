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
    public class DigEffect : ActorlessFieldTargetEffect
    {
        public override Color Color => Colors.Brown;
        public override Impact Impact => Impact.Neutral;

        public override UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            map.RemoveWalls(positions);
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            return 0;
        }

        public override float EvaluatePrice()
        {
            return 15f;
        }

        public override string Info()
        {
            return "壁を掘る\n";
        }
    }
}