using System;
using System.Collections.Generic;
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
        public bool ApplyToCharacter = true;
        public bool ApplyToItem = true;
        public bool ApplyToMoney = true;
        public bool ApplyToTrap = true;
        public bool ApplyToChest = true;
        public bool ApplyToStatue = true;
        public BreakEffect(bool applyToCharacter, bool applyToItem, bool applyToMoney, bool applyToTrap, bool applyToChest, bool applyToStatue)
        {
            ApplyToCharacter = applyToCharacter;
            ApplyToItem = applyToItem;
            ApplyToMoney = applyToMoney;
            ApplyToTrap = applyToTrap;
            ApplyToChest = applyToChest;
            ApplyToStatue = applyToStatue;
        }

        private BreakTargets Targets =>
            (ApplyToCharacter ? BreakTargets.Character : BreakTargets.None)
            | (ApplyToItem ? BreakTargets.Item : BreakTargets.None)
            | (ApplyToMoney ? BreakTargets.Money : BreakTargets.None)
            | (ApplyToTrap ? BreakTargets.Trap : BreakTargets.None)
            | (ApplyToChest ? BreakTargets.Chest : BreakTargets.None)
            | (ApplyToStatue ? BreakTargets.Statue : BreakTargets.None);

        public override UniTask Apply(IEntity target, Vector2Int position, IMap map)
        {
            if (!target.CanBeBrokenBy(Targets))
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
            if (ApplyToCharacter)
            {
                return CommonSenseParameters.MonsterMaxHealth;
            }
            else
                return 5f;
        }

        public override string Description()
        {
            var targets = new List<string>();
            if (ApplyToCharacter) targets.Add("キャラクター");
            if (ApplyToItem) targets.Add("アイテム");
            if (ApplyToMoney) targets.Add("お金");
            if (ApplyToTrap) targets.Add("罠");
            if (ApplyToChest) targets.Add("宝箱");
            if (ApplyToStatue) targets.Add("石像");
            return $"{string.Join("、", targets)}を破壊\n";
        }
    }
}