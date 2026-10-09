#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class TheftDetectedPresentation : Presentation<TheftDetected>
    {
        protected override bool WaitsForMovers(TheftDetected worldEvent) => true;

        protected override IEnumerable<ViewOp> OpsOf(TheftDetected worldEvent)
        {
            foreach (var op in ShopPrices.Hidden(worldEvent.Shop))
                yield return op;
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            foreach (var op in InventoryRows.Of(worldEvent.Underfoot))
                yield return op;
            yield return new Pause(RoomEvents.PauseSeconds);
            yield return Logs.Line("どろぼう！".Paint(Tint.Alert));
            yield return new PlayBgm(BgmTrack.Stolen);
        }
    }
}
