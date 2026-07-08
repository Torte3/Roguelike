#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat
{
    internal sealed class SkillPartiallySucceededPresentation : Presentation<SkillPartiallySucceeded>
    {
        protected override IEnumerable<ViewOp> OpsOf(SkillPartiallySucceeded worldEvent)
        {
            if (worldEvent.IsVisible)
                yield return Logs.Placed(worldEvent.FollowsActivation, $"{worldEvent.Successes}回成功した");
        }
    }
}
