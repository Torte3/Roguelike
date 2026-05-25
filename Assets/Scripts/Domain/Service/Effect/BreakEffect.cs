using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Service.Characters;
using Domain.Service.Events;
using Domain.Service.Items;
using Domain.Service.Logs;
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

        public override UniTask Apply(IEntity target, Vector2Int position, IMap map)
        {
            if (target is Character character && ApplyToCharacter)
            {
                GameLog.Add(target.IsVisible, $"{character.GetName(map.Player)}は破壊された");
            }
            else if (target is ItemEntity item && ApplyToItem)
            {
                GameLog.Add(target.IsVisible, $"{item.Item.GetName(map.Player, map.ItemPlaceholders)}は破壊された");
            }
            else if (target is Money money && ApplyToMoney)
            {
                GameLog.Add(target.IsVisible, $"{money.Amount}Gは破壊された");
            }
            else if (target is Trap trap && ApplyToTrap)
            {
                GameLog.Add(target.IsVisible, $"{trap.Name}は破壊された");
            }
            else if (target is Chest chest && ApplyToChest)
            {
                GameLog.Add(target.IsVisible, "宝箱は破壊された");
            }
            else if (target is Statue statue && ApplyToStatue)
            {
                GameLog.Add(target.IsVisible, $"{statue.Name}は破壊された");
            }
            else
            {
                return UniTask.CompletedTask;
            }

            target.Entity.Destroy("は破壊された");
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

        public override string Info()
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