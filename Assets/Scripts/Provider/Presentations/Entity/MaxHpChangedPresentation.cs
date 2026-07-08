#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class MaxHpChangedPresentation : Presentation<MaxHpChanged>
    {
        private readonly HealthCommands _healthCommands;

        public MaxHpChangedPresentation(HealthCommands healthCommands)
        {
            _healthCommands = healthCommands;
        }

        protected override Guid? SubjectOf(MaxHpChanged worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(MaxHpChanged worldEvent)
        {
            return _healthCommands.HpBarAndPlayerStatus(worldEvent.Entity, worldEvent.Health);
        }
    }
}
