using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Item.Slots;
using Domain.Model.Map;

namespace Domain.Service.ItemEffect
{
    public class Repair : IItemEffect
    {
        public bool CanApplyTo(IItem item, IActor player)
        {
            return item.RemainingUses.CurrentValue < item.MaxUsages;
        }

        public void Apply(IItemSlot slot, IEntity itemHolder, IActor player, IMap map)
        {
            slot.Item.Repair(itemHolder, map);
        }

        public float EvaluatePrice()
        {
            return 500;
        }

        public string Description()
        {
            return "修理";
        }
    }
}