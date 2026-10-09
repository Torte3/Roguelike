#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemThrownPresentation : EntityPresentation<ItemThrown>
    {
        protected override bool WaitsForMovers(ItemThrown worldEvent) => true;

        protected override IEnumerable<ViewOp> OpsOf(ItemThrown worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            yield return Flights.Fly(worldEvent.Flight);
            yield return new PlayAttack(worldEvent.Entity.Key());
            if (worldEvent.IsVisible)
                yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{Names.Of(worldEvent.ItemName)}を投げた");
        }
    }
}
