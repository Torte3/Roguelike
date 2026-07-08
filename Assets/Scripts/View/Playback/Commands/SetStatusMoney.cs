#nullable enable

namespace View.Playback.Commands
{
    public sealed record SetStatusMoney(int Money) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Status.SetMoney(Money);
        }
    }
}
