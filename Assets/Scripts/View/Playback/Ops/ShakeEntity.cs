#nullable enable

namespace View.Playback.Ops
{
    public sealed record ShakeEntity(EntityKey Key) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.Shake(Key);
        }
    }
}
