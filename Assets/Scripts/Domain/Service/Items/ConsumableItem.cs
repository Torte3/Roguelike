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
using Domain.Service.Effect;
using Domain.Service.Logs;
using R3;
using UnityEngine;
using Utilities;
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

        private void TryMarkUsedWhileCursed(IActorOfEffect actor, IMap map)
        {
            if (!IsCursed || UsedWhileCursed)
            {
                return;
            }

            UsedWhileCursed = true;
            _onItemUpdated.OnNext(Unit.Default);

            if (CurseKind == ItemCurseKind.CannotDiscardWhenCursed)
            {
                GameLog.Add(actor.IsVisible,
                    $"{GetName(map.Player, map.ItemPlaceholders)}は捨てられなくなった");
            }
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

        public override void Repair(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            GameLog.Add(itemHolder.IsVisible, $"{GetName(player, itemPlaceholders)}は修理された");
            _remainingUsages.Value = MaxUsages;
            _onItemUpdated.OnNext(Unit.Default);
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

        private void ApplyPostUseAttemptInventoryEffects(IActorOfEffect actor, IMap map)
        {
            if (ShouldDecreaseUsage(actor))
            {
                _remainingUsages.Value -= 1;
            }
            else
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は消費しなかった");
            }

            if (State == ItemState.ShopItem)
            {
                SetState(ItemState.UsedShopItem);
            }

            _onItemUpdated.OnNext(Unit.Default);
        }

        public override void LogWhyCannotActivateWhenUsed(IActor actor, IMap map)
        {
            if (!CannotUseWhileCursed || !IsCursed)
            {
                return;
            }

            GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は呪われているため使用できない");
        }

        public override async UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction, IMap map)
        {
            Debug.Log($"Use:");
            if (ShouldRevealMimic(actor, position, map))
            {
                return SpawnEffectSkillResult.Failed;
            }

            actor.KnowCurse(this, true);

            if (!actor.CanReadItem && RequiresLiteracy)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は文字が読めない");
                return SpawnEffectSkillResult.Failed;
            }

            var skill = SkillOnUse.Expect("SkillOnUse is null");

            if (!skill.IsUsable())
            {
                GameLog.Add(actor.IsVisible, $"しかしうまくいかなかった");
                TryMarkUsedWhileCursed(actor, map);
                return SpawnEffectSkillResult.Failed;
            }

            var result = await skill.Use(actor, this, position, direction, map);
            if (result.Result != SkillResult.Cancelled)
            {
                ApplyPostUseAttemptInventoryEffects(actor, map);
                TryMarkUsedWhileCursed(actor, map);
            }

            return result;
        }

        public override async UniTask<ISkillResult> UseWhenThrown(IActorOfEffect actor, Vector2Int position,
            Direction8 direction, IMap map)
        {
            if (ShouldRevealMimic(actor, position, map))
            {
                return SpawnEffectSkillResult.Failed;
            }

            if (actor is IHasInventory holder)
                holder.KnowCurse(this, true);

            if (!actor.CanReadItem && RequiresLiteracy)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は文字が読めない");
                return SpawnEffectSkillResult.Failed;
            }

            var skill = SkillOnThrow.Expect("SkillOnThrow is null");

            if (!skill.IsUsable())
            {
                return SpawnEffectSkillResult.Failed;
            }

            var result = await SkillExtension.Match(
                skill.Skill,
                spawnEffectSkill => spawnEffectSkill.Use(actor, this, position, direction, map),
                itemTargetSkill => throw new Exception(
                    "The item is not configured to activate this type of skill when thrown."),
                inventoryTargetSkill => throw new Exception(
                    "The item is not configured to activate this type of skill when thrown."),
                equipToggleSkill => equipToggleSkill.Use(actor, this, position, direction, map)
            );
            if (result.Result != SkillResult.Cancelled)
            {
                ApplyPostUseAttemptInventoryEffects(actor, map);
            }

            return result;
        }
    }
}
