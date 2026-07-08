#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Combat.SkillSources
{
    internal sealed class DeviceSkillSourcePresentation : SkillSourcePresentation<DeviceSkillSource>
    {
        protected override bool IsWaitingForMovers(SkillUsed used, DeviceSkillSource source) => false;

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(SkillUsed used, DeviceSkillSource source)
        {
            yield return AddLog.Line($"{Tint.Alert.Paint(source.DeviceName)}が起動した");
            if (source.Kind == DeviceKind.Trap)
                yield return new PlaySE(SEKind.TrapStep);
        }

        public override ViewCommand ResultLine(string text) => AddLog.Line(text);
    }
}
