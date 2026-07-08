#nullable enable

namespace View.Playback.Commands
{
    public sealed record ClearLog : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Log.Clear();
        }
    }
}
