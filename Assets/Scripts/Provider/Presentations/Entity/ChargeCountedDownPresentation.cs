#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class ChargeCountedDownPresentation : EntityPresentation<ChargeCountedDown>
    {
        protected override IEnumerable<ViewOp> OpsOf(ChargeCountedDown worldEvent)
        {
            yield return new SetChargeTurns(worldEvent.Entity.Key(), worldEvent.Turns);
        }
    }
}
