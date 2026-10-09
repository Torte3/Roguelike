#nullable enable

namespace View.Playback.Ops
{
    public sealed record PlayAttack(EntityKey Key) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.PlayAttack(Key);
        }
    }
}
