#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
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
    public abstract class EquipmentItem : BaseItem, IEquipmentToggleTarget
    {
        private readonly ISkillWithCost _skillOnUseToggle;

        protected EquipmentItem(BaseItemMemento baseItem) : base(baseItem)
        {
            _skillOnUseToggle = new SkillWithCost(new SkillWithCostMemento(
                EquipToggleSkill.BuildMemento(),
                cost: 0,
                chargeTurn: 0,
                coolTime: 0,
                remainingTurn: 0));
        }

        public override bool IsDisabled => false;

        public override Option<ISkillWithCost> SkillOnUse => Option.Some(_skillOnUseToggle);

        public override Option<ISkillWithCost> SkillOnThrow => Option.None<ISkillWithCost>();

        public void ToggleEquippedFromUse()
        {
            if (!_isEquipped.IsSome(out var rp))
                return;
            rp.Value = !rp.CurrentValue;
            _onItemUpdated.OnNext(Unit.Default);
        }

        protected void SetIsEquipped(bool isEquipped)
        {
            if (_isEquipped.IsSome(out var rp))
                rp.Value = isEquipped;
        }

        public override void Repair(IPlayer player, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
        }

        public override async UniTask<ISkillResult> Use(IActor actor, Vector2Int position, Direction8 direction,
            IMap map)
        {
            if (ShouldRevealMimic(actor, position, map))
            {
                return SpawnEffectSkillResult.Failed;
            }

            SetCurseIdentified(true);
            if (!actor.CanReadItem && RequiresLiteracy)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は文字が読めない");
                return SpawnEffectSkillResult.Failed;
            }

            var skill = SkillOnUse.Expect("SkillOnUse is null");

            if (!skill.IsUsable())
            {
                GameLog.Add(actor.IsVisible, $"しかしうまくいかなかった");
                return SpawnEffectSkillResult.Failed;
            }

            var result = await skill.Use(actor, this, position, direction, map);
            if (result.Result != SkillResult.Cancelled)
            {
                if (State == ItemState.ShopItem)
                {
                    SetState(ItemState.UsedShopItem);
                }

                _onItemUpdated.OnNext(Unit.Default);
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

            SetCurseIdentified(true);
            if (!actor.CanReadItem && RequiresLiteracy)
            {
                GameLog.Add(actor.IsVisible, $"{GetName(map.Player, map.ItemPlaceholders)}は文字が読めない");
                return SpawnEffectSkillResult.Failed;
            }

            if (!SkillOnThrow.HasValue)
            {
                return SpawnEffectSkillResult.Failed;
            }

            var skill = SkillOnThrow.Expect("SkillOnThrow is null");

            if (!skill.IsUsable())
            {
                return SpawnEffectSkillResult.Failed;
            }

            var result = await SkillExtension.Match(
                skill.Skill,
                spawnEffectSkill => spawnEffectSkill.Use(actor, position, direction, map),
                itemTargetSkill => throw new Exception(
                    "The item is not configured to activate this type of skill when thrown."),
                inventoryTargetSkill => throw new Exception(
                    "The item is not configured to activate this type of skill when thrown."),
                equipToggleSkill => equipToggleSkill.Use(actor, this, position, direction, map)
            );
            if (result.Result != SkillResult.Cancelled)
            {
                if (State == ItemState.ShopItem)
                {
                    SetState(ItemState.UsedShopItem);
                }

                _onItemUpdated.OnNext(Unit.Default);
            }

            return result;
        }
    }
}
