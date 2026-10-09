#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemPickedUpPresentation : EntityPresentation<ItemPickedUp>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemPickedUp worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            if (!worldEvent.IsVisible)
                yield break;

            var item = Names.Of(worldEvent.ItemName);
            yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{(worldEvent.IsAutomatic ? item.Paint(Tint.Notice) : item)}を拾った");
            yield return new PlaySe(SeKind.Pickup);
            yield return Obtained.Popup(worldEvent.Obtained);
        }
    }
}
