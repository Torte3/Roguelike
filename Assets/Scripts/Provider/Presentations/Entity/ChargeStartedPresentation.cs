#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class ChargeStartedPresentation : EntityPresentation<ChargeStarted>
    {
        protected override IEnumerable<ViewOp> OpsOf(ChargeStarted worldEvent)
        {
            yield return new SetCharge(worldEvent.Entity.Key(), Charges.Of(worldEvent.Charge));
        }
    }
}
