using System;
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Item.Slots;
using Domain.Model.Map;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class UpgradeItem : IItemEffect
    {
        public bool CanApplyTo(IItem item, IActor player)
        {
            return item.CanUpgrade();
        }

        public void Apply(IItemSlot slot, IEntity itemHolder, IActor player, IMap map)
        {
            slot.Item.Upgrade(itemHolder, map);
        }

        public float EvaluatePrice()
        {
            return 1000;
        }

        public string Description()
        {
            return "強化";
        }
    }
}