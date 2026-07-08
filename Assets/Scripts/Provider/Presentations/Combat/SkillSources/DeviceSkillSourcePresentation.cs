#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat.SkillSources
{
    internal sealed class DeviceSkillSourcePresentation : SkillSourcePresentation<DeviceSkillSource>
    {
        protected override bool WaitsForMovers(SkillUsed used, DeviceSkillSource source) => false;

        protected override IEnumerable<ViewOp> OpsOf(SkillUsed used, DeviceSkillSource source)
        {
            if (!used.IsVisible)
                yield break;

            yield return Logs.Line($"{source.DeviceName.Paint(Tint.Alert)}が起動した");
            if (source.Kind == DeviceKind.Trap)
                yield return new PlaySe(SeKind.TrapStep);
        }
    }
}
