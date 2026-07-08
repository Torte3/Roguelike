#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;

namespace Provider.Presentations.Combat
{
    internal sealed class ProjectileFlewPresentation : Presentation<ProjectileFlew>
    {
        private readonly ProjectileSight _projectiles;

        public ProjectileFlewPresentation(ProjectileSight projectiles)
        {
            _projectiles = projectiles;
        }

        protected override bool IsWaitingForMovers(ProjectileFlew worldEvent) => true;

        protected override IEnumerable<ViewCommand> CommandsOf(ProjectileFlew worldEvent)
        {
            return _projectiles.Flew(worldEvent.Flight);
        }
    }
}
