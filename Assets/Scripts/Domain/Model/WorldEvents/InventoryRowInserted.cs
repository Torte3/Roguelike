#nullable enable
namespace Domain.Model.WorldEvents
{
    public record InventoryRowInserted(int Index, ItemLook Item) : InventoryRowChange(Index);
}
