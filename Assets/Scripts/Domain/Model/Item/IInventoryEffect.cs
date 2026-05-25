#nullable enable
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Entity;

namespace Domain.Model.Item
{
    public interface IInventoryEffect : IHasInfo
    {
        public void Apply(IPlayer player, IStorage storage, IEntity itemHolder, ItemPlaceholders itemPlaceholders);
        public float EvaluatePrice();
    }
}