#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Combat.SkillSources;
using View.Playback;

namespace Provider.Presentations.Combat
{
    internal sealed class SkillPartiallySucceededPresentation : Presentation<SkillPartiallySucceeded>
    {
        private readonly SkillSourcePresentations _sources;

        public SkillPartiallySucceededPresentation(SkillSourcePresentations sources)
        {
            _sources = sources;
        }

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(SkillPartiallySucceeded worldEvent)
        {
            yield return _sources.Of(worldEvent.Source).ResultLine($"{worldEvent.Successes}回成功した");
        }
    }
}
