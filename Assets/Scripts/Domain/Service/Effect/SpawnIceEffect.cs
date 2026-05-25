using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class SpawnIceEffect : ActorlessFieldTargetEffect
    {
        public override Color Color => Colors.LightSkyBlue;
        public override Impact Impact => Impact.Neutral;

        public override UniTask Apply(IEnumerable<Vector2Int> positions, IMap map)
        {
            map.SetIce(positions, true);
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, IEnumerable<Vector2Int> positions)
        {
            return 50f / CommonSenseParameters.MonsterMaxHealth;
        }

        public override float EvaluatePrice()
        {
            return 50f;
        }

        public override string Info()
        {
            return "水上なら氷を生成\n";
        }
    }
}