using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Item;

namespace Domain.Service.ItemEffect
{
    public class CurseItem : IItemEffect
    {
        public bool CanApplyTo(IPlayer player, IItem item)
        {
            return !item.IsCursed || (!player.Character.IsKnownItem(item) && !item.IsCurseIdentified);
        }

        public void Apply(IPlayer player, IItem item, IEntity itemHolder, ItemPlaceholders itemPlaceholders)
        {
            item.SetCursed(player, itemHolder, itemPlaceholders, true);
        }

        public float EvaluatePrice()
        {
            return 100;
        }

        public string Info() => "呪い付与";
    }
}