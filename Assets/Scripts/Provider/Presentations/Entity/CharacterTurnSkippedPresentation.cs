#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterTurnSkippedPresentation : Presentation<CharacterTurnSkipped>
    {
        private readonly PlayerEntity _player;

        public CharacterTurnSkippedPresentation(PlayerEntity player)
        {
            _player = player;
        }

        private const float PlayerPauseSeconds = 0.2f;

        protected override Guid? SubjectOf(CharacterTurnSkipped worldEvent) => KeyOf(worldEvent.Entity);

        protected override bool IsWaitingForMovers(CharacterTurnSkipped worldEvent)
        {
            return _player.Is(worldEvent.Entity);
        }

        protected override IEnumerable<ViewCommand> CommandsOf(CharacterTurnSkipped worldEvent)
        {
            if (_player.Is(worldEvent.Entity))
                yield return new Pause(PlayerPauseSeconds);
        }
    }
}
