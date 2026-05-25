using System;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    [Serializable]
    public class UpgradeItem : IItemEffect
    {
        public bool CanApplyTo(IPlayer player, IItem item)
        {
            return item.CanUpgrade();
        }

        public void Apply(IPlayer player, IItem item, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            item.Upgrade(player, itemHolder, itemPlaceholders);
        }

        public float EvaluatePrice()
        {
            return 1000;
        }

        public string Info()
        {
            return "強化";
        }
    }
}