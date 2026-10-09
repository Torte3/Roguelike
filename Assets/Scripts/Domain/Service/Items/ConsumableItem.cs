#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Evaluation;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using Domain.Service.Effect;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Result;
using Utilities.Serialize.Option;

namespace Domain.Service.Items
{
    public abstract class ConsumableItem : BaseItem
    {
        public override ReadOnlyReactiveProperty<bool> IsPassiveActive { get; }

        protected ConsumableItem(BaseItemMemento baseItem) : base(baseItem)
        {
            IsPassiveActive = _isCursed
                .Select(c => !(CannotUseWhileCursed && c))
                .DistinctUntilChanged()
                .ToReadOnlyReactiveProperty();
            IsPassiveActive.AddTo(_disposables);
        }

        public override bool IsDiscardBlocked =>
            IsCursed && !CannotUseWhileCursed && UsedWhileCursed;

        public override Option<bool> IsEquipped => Option.None<bool>();

        public abstract ItemCurseKind CurseKind { get; }

        protected bool CannotUseWhileCursed => CurseKind switch
        {
            ItemCurseKind.UseBlockedWhenCursed => true,
            ItemCurseKind.CannotDiscardWhenCursed => false,
        };

        private protected override void OnUseAttemptedInHand(IActorOfEffect actor, IMap map)
        {
            if (!IsCursed || UsedWhileCursed)
            {
                return;
            }

            UsedWhileCursed = true;
            RecordChange(actor, map, CurseKind == ItemCurseKind.CannotDiscardWhenCursed
                ? ItemChangeKind.BecameUndiscardable
                : ItemChangeKind.UsedWhileCursed);
        }

        private bool IsUseBlockedByCurse =>
            CurseKind == ItemCurseKind.UseBlockedWhenCursed && IsCursed;

        public override bool CanActivateWhenUsed =>
            HasUsableSkillOnUse() && RemainingUses.CurrentValue > 0 && !IsUseBlockedByCurse;

        public override bool CanActivateWhenThrown =>
            HasUsableSkillOnThrow() && RemainingUses.CurrentValue > 0 && !IsUseBlockedByCurse;

        public override bool CanAttemptUse =>
            HasUsableSkillOnUse() && RemainingUses.CurrentValue > 0;

        public override bool CanAttemptThrow => !IsDiscardBlocked;

        public override void Repair(IEntity itemHolder, IMap map)
        {
            var name = this.NameIn(map);
            _remainingUsages.Value = MaxUsages;
            RecordChange(itemHolder, map, ItemChangeKind.Repaired, name);
        }

        private bool ShouldDecreaseUsage(IActorOfEffect actor)
        {
            if (Category == ItemCategory.Books
                && actor.Status.IsFlagStat(FlagStatType.BookMaster)
                && RandUtils.IsGreaterThanProbability(CommonSenseParameters.BookMasterUsageLossChance))
                return false;
            if (Category == ItemCategory.Wands
                && actor.Status.IsFlagStat(FlagStatType.WandMaster)
                && RandUtils.IsGreaterThanProbability(CommonSenseParameters.WandMasterUsageLossChance))
                return false;
            return RandUtils.IsLessThanProbability(UsageLossChance);
        }

        private bool BreaksWhenConsumed => MaxUsages > 1 && _remainingUsages.Value == 1;

        private protected override bool ConfirmUse(IActorOfEffect actor, IMap map)
        {
            var isConsumed = ShouldDecreaseUsage(actor);
            if (isConsumed && BreaksWhenConsumed)
                map.Events.Record(new ItemBroken(actor.Entity.IsVisible, this.NameIn(map)));
            return isConsumed;
        }

        private protected override void FinishUse(IActorOfEffect actor, IMap map, bool isConsumed)
        {
            if (isConsumed)
            {
                _remainingUsages.Value -= 1;
            }

            if (State == ItemState.ShopItem)
            {
                SetState(ItemState.UsedShopItem);
            }

            RecordChange(actor, map, isConsumed ? ItemChangeKind.Consumed : ItemChangeKind.NotConsumed);
        }

        public override Result<Unit, ItemActionFailure> ActivationCheckWhenUsed()
        {
            return ItemChecks.Check(!CannotUseWhileCursed || !IsCursed, ItemActionFailure.CursedCannotUse);
        }
    }
}
