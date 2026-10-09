#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat
{
    internal sealed class SkillFailedPresentation : Presentation<SkillFailed>
    {
        protected override IEnumerable<ViewOp> OpsOf(SkillFailed worldEvent)
        {
            if (!worldEvent.IsVisible)
                yield break;

            yield return Logs.Placed(worldEvent.FollowsActivation, worldEvent.Kind switch
            {
                SkillFailureKind.NoEffect => "しかし効果がなかった",
                SkillFailureKind.ItemUnaffected => "しかし効果はなかった。",
                SkillFailureKind.Fizzled => "しかしうまくいかなかった",
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent), worldEvent.Kind, null),
            });
        }
    }
}
