#nullable enable
namespace Domain.Model.WorldEvents
{
    public record InventoryRowRemoved(int Index) : InventoryRowChange(Index);
}
