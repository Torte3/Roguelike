#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetEntityVisibility(EntityKey Key, bool IsVisible) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetVisibility(Key, IsVisible);
        }
    }
}
