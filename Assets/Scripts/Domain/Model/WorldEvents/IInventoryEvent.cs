#nullable enable
namespace Domain.Model.WorldEvents
{
    public interface IInventoryEvent
    {
        public InventoryLook? Inventory { get; }
    }
}
