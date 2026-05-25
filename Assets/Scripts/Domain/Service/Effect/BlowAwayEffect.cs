using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class BlowAwayEffect : EntityTargetEffect
    {
        [MinValue(1)] [SerializeField] private int _distance;

        public BlowAwayEffect(int distance)
        {
            _distance = distance;
        }

        public override Color Color => Colors.LightGreen;

        public override Impact Impact => Impact.Harmful;

        public override async UniTask Apply(IActorOfEffect actor, ITargetOfEffect target, Vector2Int position, IMap map)
        {
            if (!target.Status.IsFlagStat(FlagStatType.Heavy))
            {
                await Apply(actor, (IEntity)target, position, map);
            }
        }

        public override async UniTask Apply(IActorOfEffect actor, IEntity target, Vector2Int position, IMap map)
        {
            var direction =
                DirectionMethods.NearestDirectionFromVector(
                    target.Entity.CurrentPosition - actor.Entity.CurrentPosition);
            if (direction.HasValue)
                await target.BlowAway(actor, direction.Value, _distance, map);
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            if (target.Status.IsFlagStat(FlagStatType.Heavy))
            {
                return 0f;
            }

            return CommonSenseParameters.BlowAwayEvaluate(_distance);
        }

        public override float EvaluatePrice()
        {
            return CommonSenseParameters.BlowAwayPrice(_distance);
        }

        public override string Info()
        {
            return $"{_distance}マス吹き飛ばす\n";
        }
    }
}