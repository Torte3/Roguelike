#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Combat.SkillSources;
using View.Playback;

namespace Provider.Presentations.Combat
{
    internal sealed class SkillUsedPresentation : EntityPresentation<SkillUsed>
    {
        protected override bool WaitsForMovers(SkillUsed worldEvent) => SkillSourcePresentations.Of(worldEvent.Source).WaitsForMovers(worldEvent);

        protected override IEnumerable<ViewOp> OpsOf(SkillUsed worldEvent) =>
            SkillSourcePresentations.Of(worldEvent.Source).OpsOf(worldEvent);
    }
}
