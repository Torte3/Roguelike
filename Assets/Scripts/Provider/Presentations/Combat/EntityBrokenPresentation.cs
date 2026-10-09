#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat
{
    internal sealed class EntityBrokenPresentation : Presentation<EntityBroken>
    {
        protected override IEnumerable<ViewOp> OpsOf(EntityBroken worldEvent)
        {
            if (worldEvent.IsVisible)
                yield return Logs.Line($"{Names.Of(worldEvent.Target)}は破壊された");
        }
    }
}
