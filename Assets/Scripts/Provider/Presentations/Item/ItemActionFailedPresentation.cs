#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;

namespace Provider.Presentations.Item
{
    internal sealed class ItemActionFailedPresentation : Presentation<ItemActionFailed>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemActionFailed worldEvent)
        {
            if (worldEvent.IsVisible)
                yield return Logs.Line(FailureText.Of(worldEvent));
        }
    }
}
