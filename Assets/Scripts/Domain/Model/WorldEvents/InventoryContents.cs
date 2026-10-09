#nullable enable
using System.Collections.Generic;

namespace Domain.Model.WorldEvents
{
    public record InventoryContents(IReadOnlyList<InventoryRowChange> Changes, IReadOnlyList<InventoryRow> Rows, int Count,
        int Capacity, bool CanRemoveItems);
}
