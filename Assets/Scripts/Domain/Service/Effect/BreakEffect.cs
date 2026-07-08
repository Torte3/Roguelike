using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Service.Effect
{
    [Serializable]
    public class BreakEffect : ActorlessEntityTargetEffect
    {
        public override Color Color => Colors.DarkGray;
        public override Impact Impact => Impact.Harmful;
        private static readonly IReadOnlyList<(BreakTargets Target, string Name)> TargetNames = new[]
        {
            (BreakTargets.Character, "キャラクター"),
            (BreakTargets.Item, "アイテム"),
            (BreakTargets.Money, "お金"),
            (BreakTargets.Trap, "罠"),
            (BreakTargets.Chest, "宝箱"),
            (BreakTargets.Statue, "石像"),
        };

        [SerializeField] private BreakTargets _targets;

        public BreakEffect(BreakTargets targets)
        {
            _targets = targets;
        }

        public override UniTask Apply(IEntity target, Vector2Int position, IMap map)
        {
            if (!target.CanBeBrokenBy(_targets))
                return UniTask.CompletedTask;

            target.Break(map);
            return UniTask.CompletedTask;
        }

        public override float Evaluate(IActorOfEffect actor, ITargetOfEffect target)
        {
            return 1;
        }

        public override float EvaluatePrice()
        {
            if (_targets.HasFlag(BreakTargets.Character))
            {
                return CommonSenseParameters.MonsterMaxHealth;
            }
            else
                return 5f;
        }

        public override string Description()
        {
            var targets = TargetNames.Where(pair => _targets.HasFlag(pair.Target)).Select(pair => pair.Name);
            return $"{string.Join("、", targets)}を破壊\n";
        }
    }
}