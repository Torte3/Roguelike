#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat
{
    internal sealed class ConditionResistedPresentation : EntityPresentation<ConditionResisted>
    {
        protected override IEnumerable<ViewOp> OpsOf(ConditionResisted worldEvent)
        {
            if (worldEvent.IsVisible)
                yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{worldEvent.ConditionName}に耐性がある");
        }
    }
}
