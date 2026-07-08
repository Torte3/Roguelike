#nullable enable
using Domain.Model.Entity;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public record InventoryLook(Id<IEntity> Holder, bool HolderAppearsInteractable, InventoryContents? Contents);
}
