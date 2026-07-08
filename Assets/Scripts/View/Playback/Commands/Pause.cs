#nullable enable

namespace View.Playback.Commands
{
    public sealed record Pause(float Seconds) : ViewCommand
    {
        internal override float ApplyAndGetBlockSeconds(PlaybackContext context)
        {
            return Seconds;
        }
    }
}
