#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetKeyHolder(EntityKey Key, bool IsKeyHolder) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetKeyHolder(Key, IsKeyHolder);
        }
    }
}
