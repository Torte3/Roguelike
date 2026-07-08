#nullable enable
using Domain.Model.WorldEvents;
using Provider.Presentations.Combat.SkillSources;
using System;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Combat
{
    internal sealed class SkillFailedPresentation : Presentation<SkillFailed>
    {
        private readonly SkillSourcePresentations _sources;

        public SkillFailedPresentation(SkillSourcePresentations sources)
        {
            _sources = sources;
        }

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(SkillFailed worldEvent)
        {
            yield return worldEvent.Kind switch
            {
                SkillFailureKind.NoEffect => _sources.Of(worldEvent.Source).ResultLine("しかし効果がなかった"),
                SkillFailureKind.Fizzled => AddLog.Line("しかしうまくいかなかった"),
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent.Kind), worldEvent.Kind, null),
            };
        }
    }
}
