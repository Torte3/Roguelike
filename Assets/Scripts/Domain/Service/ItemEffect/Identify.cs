using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class Identify : IItemEffect
    {
        public bool CanApplyTo(IPlayer player, IItem item)
        {
            return !player.Character.IsKnownItem(item);
        }

        public void Apply(IPlayer player, IItem item, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            player.Character.KnowItem(item, true);
            item.SetCurseIdentified(true, player, itemHolder, itemPlaceholders);
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Info()
        {
            return "識別";
        }
    }
}