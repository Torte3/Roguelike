#nullable enable
using System;
using Utilities;

namespace View.Playback.Commands
{
    public sealed record TurnCharacter(Guid Key, Direction8 Direction) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.Turn(Key, Direction);
        }
    }
}
