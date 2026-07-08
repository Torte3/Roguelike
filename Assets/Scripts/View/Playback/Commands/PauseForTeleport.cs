#nullable enable
using Configuration;

namespace View.Playback.Commands
{
    public sealed record PauseForTeleport : ViewCommand
    {
        internal override float ApplyAndGetBlockSeconds(PlaybackContext context)
        {
            return Settings.GlobalSettings.MoveMilliseconds.CurrentValue / 1000f;
        }
    }
}
