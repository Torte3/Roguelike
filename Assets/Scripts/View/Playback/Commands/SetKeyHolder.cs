#nullable enable
using System;

namespace View.Playback.Commands
{
    public sealed record SetKeyHolder(Guid Key, bool IsKeyHolder) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetKeyHolder(Key, IsKeyHolder);
        }
    }
}
