#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Logs;

namespace Domain.Service.Effect
{
    public class ItemTargetSkill : ISerializable<ItemTargetSkillMemento>, ISkill
    {
        private readonly IItemEffect _itemEffect;
        public bool IsDirectional => false;
        public bool IsUsable() => true;

        public ItemTargetSkill(ItemTargetSkillMemento memento)
        {
            _itemEffect = memento.ItemEffect;
        }

        public ItemTargetSkillMemento Serialize()
        {
            return new ItemTargetSkillMemento
            (
                _itemEffect
            );
        }

        public static ItemTargetSkillMemento Build(IItemEffect itemEffect)
        {
            return new ItemTargetSkillMemento
            (
                itemEffect
            );
        }

        private ItemFocus GetItemIndex(IPlayer player, IItem item, IMap map)
        {
            var selfIndex = player.Character.Inventory.GetItemIndex(item);
            if (selfIndex != null)
            {
                return new ItemFocus(selfIndex.Value);
            }

            var groundItem = map.Items.At(player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            if (item == groundItem)
            {
                return ItemFocus.GroundItem;
            }

            throw new Exception("ItemTargetSkill: Item not found in inventory or ground.");
        }

        public async UniTask<ISkillResult> Use(IPlayer player, IItem item, IEntity itemHolder, IMap map)
        {
            var selfIndex = GetItemIndex(player, item, map);

            var disabledItemIndexes = new List<ItemFocus>();
            foreach (var (item2, index) in player.Character.Inventory.AllItemsWithIndex)
            {
                if (!_itemEffect.CanApplyTo(player, item2))
                {
                    disabledItemIndexes.Add(new ItemFocus(index));
                }
            }

            var groundItem = map.Items.At(player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            if (groundItem == null || !_itemEffect.CanApplyTo(player, groundItem))
            {
                disabledItemIndexes.Add(ItemFocus.GroundItem);
            }

            disabledItemIndexes.Add(selfIndex);
            if (player.Character.IsKnownItem(item))
            {
                var focus = await player.Character.SelectItemContainsGroundItem("適応するアイテムを選択してください",
                    disabledItemIndexes.ToArray());
                if (focus.IsOnItem(player.Character.Inventory, map, out var selectedItem))
                {
                    _itemEffect.Apply(player, selectedItem, itemHolder, map.ItemPlaceholders);
                    return ItemTargetSkillResult.Success;
                }
            }
            else
            {
                var focus =
                    await player.Character.SelectItemContainsGroundItem("適応するアイテムを選択してください", selfIndex);
                if (focus.IsOnItem(player.Character.Inventory, map, out var selectedItem))
                {
                    if (disabledItemIndexes.Contains(focus))
                    {
                        GameLog.Add(itemHolder.IsVisible, "しかし効果はなかった。");
                    }
                    else
                    {
                        _itemEffect.Apply(player, selectedItem, itemHolder, map.ItemPlaceholders);
                    }

                    return ItemTargetSkillResult.Success;
                }
            }

            return ItemTargetSkillResult.Cancelled;
        }

        public float Evaluate() => 0;

        public float EvaluatePrice()
        {
            return _itemEffect.EvaluatePrice();
        }

        public string Info() =>
            "アイテムを対象に\n" + _itemEffect.Info();
    }
}