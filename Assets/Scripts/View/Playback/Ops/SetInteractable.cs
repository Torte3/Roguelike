#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetInteractable(EntityKey Key, bool IsInteractable) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetInteractable(Key, IsInteractable);
        }
    }
}
