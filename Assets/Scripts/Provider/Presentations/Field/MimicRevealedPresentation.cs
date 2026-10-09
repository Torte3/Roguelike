#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class MimicRevealedPresentation : Presentation<MimicRevealed>
    {
        protected override IEnumerable<ViewOp> OpsOf(MimicRevealed worldEvent)
        {
            foreach (var op in InventoryRows.Updated(worldEvent.Inventory))
                yield return op;
            if (worldEvent.IsVisible)
                yield return Logs.Line($"{Names.Of(worldEvent.Disguise)}は{worldEvent.MimicName}の擬態だった！");
        }
    }
}
