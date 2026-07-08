#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class ChargeEndedPresentation : EntityPresentation<ChargeEnded>
    {
        protected override IEnumerable<ViewOp> OpsOf(ChargeEnded worldEvent)
        {
            yield return new SetCharge(worldEvent.Entity.Key(), null);
        }
    }
}
