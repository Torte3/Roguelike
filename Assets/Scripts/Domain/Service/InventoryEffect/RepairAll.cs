using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.InventoryEffect
{
    public class RepairAll : IInventoryEffect
    {
        public void Apply(IStorage storage, IEntity itemHolder, IMap map)
        {
            foreach (var item in storage.AllItems)
            {
                item.Repair(itemHolder, map);
            }
        }

        public float EvaluatePrice()
        {
            return 500 * 5;
        }

        public string Description()
        {
            return "修理(全て)";
        }
    }
}