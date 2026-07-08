using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.InventoryEffect
{
    public class CurseAll : IInventoryEffect
    {
        public void Apply(IStorage storage, IEntity itemHolder, IMap map)
        {
            foreach (var item in storage.AllItems)
            {
                item.SetCursed(itemHolder, map, true);
            }
        }

        public float EvaluatePrice()
        {
            return 100 * 5;
        }

        public string Description()
        {
            return "呪い付与(全て)";
        }
    }
}