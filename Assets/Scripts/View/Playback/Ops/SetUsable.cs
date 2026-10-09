#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetUsable(EntityKey Key, bool IsUsable) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetUsable(Key, IsUsable);
        }
    }
}
