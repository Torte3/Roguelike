using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class Repair : IItemEffect
    {
        public bool CanApplyTo(IPlayer player, IItem item)
        {
            return item.RemainingUses.CurrentValue < item.MaxUsages;
        }

        public void Apply(IPlayer player, IItem item, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            item.Repair(player, itemHolder, itemPlaceholders);
        }

        public float EvaluatePrice()
        {
            return 500;
        }

        public string Info()
        {
            return "修理";
        }
    }
}