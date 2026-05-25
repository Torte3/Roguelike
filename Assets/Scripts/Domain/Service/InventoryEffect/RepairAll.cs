using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;

namespace Domain.Service.InventoryEffect
{
    public class RepairAll : IInventoryEffect
    {
        public void Apply(IPlayer player, IStorage storage, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            foreach (var item in storage.AllItems)
            {
                item.Repair(player, itemHolder, itemPlaceholders);
            }
        }

        public float EvaluatePrice()
        {
            return 500 * 5;
        }

        public string Info()
        {
            return "修理(全て)";
        }
    }
}