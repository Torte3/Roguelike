#nullable enable
using System.Collections.Generic;

namespace Domain.Model.WorldEvents
{
    public record InventoryContents(IReadOnlyList<ItemLook> Items, int Capacity, bool CanRemoveItems);
}
