#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat.SkillSources
{
    internal sealed class CharacterSkillSourcePresentation : SkillSourcePresentation<CharacterSkillSource>
    {
        protected override bool WaitsForMovers(SkillUsed used, CharacterSkillSource source) => used.IsVisible;

        protected override IEnumerable<ViewOp> OpsOf(SkillUsed used, CharacterSkillSource source)
        {
            yield return new PlayAttack(used.Entity.Key());
            if (used.IsVisible && source.Log != "")
                yield return Logs.Line($"{Names.Of(source.Label)}{source.Log}");
        }
    }
}
