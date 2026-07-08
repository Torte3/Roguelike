#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Field
{
    internal sealed class DeviceBrokenPresentation : Presentation<DeviceBroken>
    {
        protected override IEnumerable<ViewCommand> VisibleCommandsOf(DeviceBroken worldEvent)
        {
            yield return AddLog.Line($"{Tint.Alert.Paint(worldEvent.DeviceName)}は壊れた");
        }
    }
}
