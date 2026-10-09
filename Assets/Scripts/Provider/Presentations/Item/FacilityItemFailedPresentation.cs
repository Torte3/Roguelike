#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations.Item
{
    internal sealed class FacilityItemFailedPresentation : Presentation<FacilityItemFailed>
    {
        protected override IEnumerable<ViewOp> OpsOf(FacilityItemFailed worldEvent)
        {
            yield return Logs.Line(FailureText.Of(worldEvent));
        }
    }
}
