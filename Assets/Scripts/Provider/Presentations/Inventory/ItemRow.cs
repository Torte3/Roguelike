#nullable enable
using Domain.Model.Item;
using Domain.Model.WorldEvents;
using UnityEngine;
using View.UI.Inventory;

namespace Provider.Presentations.Inventory
{
    internal sealed class ItemRow : IItemRow
    {
        private readonly ItemLook _item;

        public ItemRow(ItemLook item, string name, string info, bool canSelect)
        {
            _item = item;
            Name = name;
            Info = info;
            CanSelect = canSelect;
        }

        public string Name { get; }
        public bool IsUsable => _item.CanAttemptUseOrThrow;
        public Sprite Icon => _item.Icon;
        public bool CanSelect { get; }
        public int? Count => _item.EquipState != EquipState.NotEquipment || !_item.HasActivatableSkill ? null : _item.RemainingUses;
        public bool HasEquippedBadge => _item.EquipState == EquipState.Equipped;
        public bool IsCursed => _item.IsCursed;
        public bool IsShiny => _item.IsShiny;
        public bool IsCountIdentified => _item.IsKnown;
        public bool IsCurseIdentified => _item.IsCurseIdentified;
        public string Info { get; }
    }
}
