#nullable enable
namespace Domain.Model.Item.Slots
{
    public interface IItemSlot
    {
        public IItem Item { get; }
        public void Replace(IItem item);
    }
}
