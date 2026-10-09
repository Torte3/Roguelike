#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetStatusMoney(int Money) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Status.SetMoney(Money);
        }
    }
}
