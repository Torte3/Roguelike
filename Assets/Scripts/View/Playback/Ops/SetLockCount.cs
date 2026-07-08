#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetLockCount(EntityKey Key, int LockCount) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetLockCount(Key, LockCount);
        }
    }
}
