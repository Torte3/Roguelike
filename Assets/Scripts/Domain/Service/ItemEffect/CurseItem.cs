using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;

namespace Domain.Service.ItemEffect
{
    public class CurseItem : IItemEffect
    {
        public bool CanApplyTo(IItem item, IMap map)
        {
            return !item.IsCursed || (!map.Player.Character.IsKnownItem(item) && !item.IsCurseIdentified);
        }

        public void Apply(IItem item, IEntity itemHolder, IMap map)
        {
            item.SetCursed(itemHolder, map, true);
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Description() => "呪い付与";
    }
}