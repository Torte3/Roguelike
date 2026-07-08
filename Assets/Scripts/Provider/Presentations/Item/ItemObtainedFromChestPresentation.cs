#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemObtainedFromChestPresentation : Presentation<ItemObtainedFromChest>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemObtainedFromChest worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{Names.Of(worldEvent.ItemName)}を手に入れた");
            yield return Obtained.Popup(worldEvent.Obtained);
        }
    }
}
