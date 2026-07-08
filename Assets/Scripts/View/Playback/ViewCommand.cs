#nullable enable
using System;

namespace View.Playback
{
    public abstract record ViewCommand
    {
        internal abstract float ApplyAndGetBlockSeconds(PlaybackContext context);

        internal virtual bool IsWalkOf(Guid key) => false;
    }
}
