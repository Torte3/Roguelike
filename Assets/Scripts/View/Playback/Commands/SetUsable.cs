#nullable enable
using System;

namespace View.Playback.Commands
{
    public sealed record SetUsable(Guid Key, bool IsUsable) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetUsable(Key, IsUsable);
        }
    }
}
