#nullable enable

namespace View.Playback.Ops
{
    public sealed record RemoveEntity(EntityKey Key) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.Remove(Key);
        }
    }
}
