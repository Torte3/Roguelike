#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetCharge(EntityKey Key, ChargePreview? Charge) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetCharge(Key, Charge);
        }
    }
}
