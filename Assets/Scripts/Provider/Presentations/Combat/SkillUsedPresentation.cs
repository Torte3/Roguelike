#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Combat.SkillSources;
using View.Playback;

namespace Provider.Presentations.Combat
{
    internal sealed class SkillUsedPresentation : Presentation<SkillUsed>
    {
        private readonly SkillSourcePresentations _sources;

        public SkillUsedPresentation(SkillSourcePresentations sources)
        {
            _sources = sources;
        }

        protected override Guid? SubjectOf(SkillUsed worldEvent) => KeyOf(worldEvent.Entity);

        protected override bool IsWaitingForMovers(SkillUsed worldEvent) => _sources.Of(worldEvent.Source).IsWaitingForMovers(worldEvent);

        protected override IEnumerable<ViewCommand> CommandsOf(SkillUsed worldEvent) =>
            _sources.Of(worldEvent.Source).CommandsOf(worldEvent);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(SkillUsed worldEvent) =>
            _sources.Of(worldEvent.Source).VisibleCommandsOf(worldEvent);
    }
}
