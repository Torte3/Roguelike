#nullable enable
namespace View.Playback.Commands
{
    public sealed record Flash : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Flush.Flush(context.Settings.FlushDuration.CurrentValue);
        }
    }
}
