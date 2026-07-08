#nullable enable
using Domain.Model.Character;
using Domain.Model.Entity;
using Domain.Model.Item.Slots;
using Domain.Model.Map;

namespace Domain.Model.Item
{
    public interface IItemEffect
    {
        public bool CanApplyTo(IItem item, IActor player);
        public void Apply(IItemSlot slot, IEntity itemHolder, IActor player, IMap map);
        public float EvaluatePrice();
        public string Description();
    }
}