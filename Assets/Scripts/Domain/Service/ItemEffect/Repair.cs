using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.ItemEffect
{
    public class Repair : IItemEffect
    {
        public bool CanApplyTo(IItem item, IMap map)
        {
            return item.RemainingUses.CurrentValue < item.MaxUsages;
        }

        public void Apply(IItem item, IEntity itemHolder, IMap map)
        {
            item.Repair(itemHolder, map);
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