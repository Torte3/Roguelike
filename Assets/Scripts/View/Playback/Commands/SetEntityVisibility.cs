#nullable enable
using System;

namespace View.Playback.Commands
{
    public sealed record SetEntityVisibility(Guid Key, bool IsVisible) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetVisibility(Key, IsVisible);
        }
    }
}
