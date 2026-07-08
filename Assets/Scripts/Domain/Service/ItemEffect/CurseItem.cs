using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Item.Slots;
using Domain.Model.Map;

namespace Domain.Service.ItemEffect
{
    public class CurseItem : IItemEffect
    {
        public bool CanApplyTo(IItem item, IActor player)
        {
            return !item.IsCursed || (!player.IsKnownItem(item) && !item.IsCurseIdentified);
        }

        public void Apply(IItemSlot slot, IEntity itemHolder, IActor player, IMap map)
        {
            slot.Item.SetCursed(itemHolder, map, true);
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Description() => "呪い付与";
    }
}