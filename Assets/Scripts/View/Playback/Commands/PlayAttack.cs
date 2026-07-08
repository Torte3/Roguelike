#nullable enable
using System;

namespace View.Playback.Commands
{
    public sealed record PlayAttack(Guid Key) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.PlayAttack(Key);
        }
    }
}
