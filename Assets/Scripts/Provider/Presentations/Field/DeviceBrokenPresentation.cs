#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Field
{
    internal sealed class DeviceBrokenPresentation : Presentation<DeviceBroken>
    {
        protected override IEnumerable<ViewOp> OpsOf(DeviceBroken worldEvent)
        {
            if (worldEvent.IsVisible)
                yield return Logs.Line($"{worldEvent.DeviceName.Paint(Tint.Alert)}は壊れた");
        }
    }
}
