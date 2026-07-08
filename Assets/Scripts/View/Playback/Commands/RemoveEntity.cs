#nullable enable
using System;

namespace View.Playback.Commands
{
    public sealed record RemoveEntity(Guid Key) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.Remove(Key);
        }
    }
}
