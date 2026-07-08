#nullable enable
namespace Domain.Model.WorldEvents
{
    public record InventoryRowChange(InventoryRowChangeKind Kind, int Index);
}
