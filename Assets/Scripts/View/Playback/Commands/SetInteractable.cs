#nullable enable
using System;

namespace View.Playback.Commands
{
    public sealed record SetInteractable(Guid Key, bool IsInteractable) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetInteractable(Key, IsInteractable);
        }
    }
}
