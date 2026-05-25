using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;

namespace Domain.Service.InventoryEffect
{
    public class CurseAll : IInventoryEffect
    {
        public void Apply(IPlayer player, IStorage storage, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            foreach (var item in storage.AllItems)
            {
                item.SetCursed(player, itemHolder, itemPlaceholders, true);
            }
        }

        public float EvaluatePrice()
        {
            return 100 * 5;
        }

        public string Info()
        {
            return "呪い付与(全て)";
        }
    }
}