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
using Domain.Model.WorldEvents;
using R3;
using Utilities.Result;

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

        public async UniTask<Result<SkillExecution, Unit>> Prepare(IItem item, IEntity itemHolder, IMap map)
        {
            var player = map.Player;
            var selfIndex = GetItemIndex(player, item, map);

            var disabledItemIndexes = new List<ItemFocus>();
            foreach (var (item2, index) in player.Character.Inventory.AllItemsWithIndex)
            {
                if (!_itemEffect.CanApplyTo(item2, map))
                {
                    disabledItemIndexes.Add(new ItemFocus(index));
                }
            }

            var groundItem = map.Items.At(player.Character.Entity.CurrentPosition).FirstOrDefault()?.Item;
            if (groundItem == null || !_itemEffect.CanApplyTo(groundItem, map))
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
                    return Result<SkillExecution, Unit>.Ok(() =>
                    {
                        _itemEffect.Apply(selectedItem, itemHolder, map);
                        return UniTask.FromResult<ISkillResult>(SkillOutcome.Success);
                    });
                }
            }
            else
            {
                var focus =
                    await player.Character.SelectItemContainsGroundItem("適応するアイテムを選択してください", selfIndex);
                if (focus.IsOnItem(player.Character.Inventory, map, out var selectedItem))
                {
                    return Result<SkillExecution, Unit>.Ok(() =>
                    {
                        if (disabledItemIndexes.Contains(focus))
                        {
                            map.Events.Record(new SkillFailed(itemHolder.Entity.IsVisible, SkillFailureKind.ItemUnaffected, false));
                        }
                        else
                        {
                            _itemEffect.Apply(selectedItem, itemHolder, map);
                        }

                        return UniTask.FromResult<ISkillResult>(SkillOutcome.Success);
                    });
                }
            }

            return Result<SkillExecution, Unit>.Cancelled();
        }

        public float Evaluate() => 0;

        public float EvaluatePrice()
        {
            return _itemEffect.EvaluatePrice();
        }

        public string Description() =>
            "アイテムを対象に\n" + _itemEffect.Description();
    }
}