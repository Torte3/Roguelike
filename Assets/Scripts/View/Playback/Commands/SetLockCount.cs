#nullable enable
using System;

namespace View.Playback.Commands
{
    public sealed record SetLockCount(Guid Key, int LockCount) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetLockCount(Key, LockCount);
        }
    }
}
