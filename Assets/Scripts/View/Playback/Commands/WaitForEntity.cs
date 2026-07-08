#nullable enable
using System;

namespace View.Playback.Commands
{
    public sealed record WaitForEntity(Guid Key) : ViewCommand
    {
        internal override float ApplyAndGetBlockSeconds(PlaybackContext context)
        {
            return context.Clock.RemainingBusySeconds(Key);
        }
    }
}
