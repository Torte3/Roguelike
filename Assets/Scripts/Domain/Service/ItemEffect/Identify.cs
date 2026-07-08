using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Item.Slots;
using Domain.Model.Map;
using Domain.Model.WorldEvents;

namespace Domain.Service.ItemEffect
{
    public class Identify : IItemEffect
    {
        public bool CanApplyTo(IItem item, IActor player)
        {
            return !player.IsKnownItem(item) || !player.IsCurseKnown(item);
        }

        public void Apply(IItemSlot slot, IEntity itemHolder, IActor player, IMap map)
        {
            player.KnowItem(slot.Item, DiscoveryCause.Scroll);
            player.KnowCurse(slot.Item, DiscoveryCause.Scroll);
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Description()
        {
            return "識別";
        }
    }
}