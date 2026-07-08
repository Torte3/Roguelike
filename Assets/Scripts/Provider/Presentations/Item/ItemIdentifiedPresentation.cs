#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemIdentifiedPresentation : Presentation<ItemIdentified>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemIdentified worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            if (worldEvent.IsVisible && worldEvent.IsAnnounced)
                yield return Logs.Line(
                    $"{ItemNameText.Unidentified(worldEvent.UnidentifiedName)}は{worldEvent.IdentifiedName.RevealedName}だった");
        }
    }
}
