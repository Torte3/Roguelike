#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemExchangedPresentation : EntityPresentation<ItemExchanged>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemExchanged worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            if (!worldEvent.IsVisible)
                yield break;

            var name = Names.Of(worldEvent.Label);
            yield return Logs.Line($"{name}は{Names.Of(worldEvent.PickedUpName)}を拾った");
            yield return Logs.Line($"{name}は{Names.Of(worldEvent.DroppedName)}を捨てた");
            yield return new PlaySe(SeKind.Pickup);
            yield return Obtained.Popup(worldEvent.Obtained);
        }
    }
}
