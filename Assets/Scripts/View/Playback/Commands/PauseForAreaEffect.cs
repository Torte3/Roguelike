#nullable enable
using Configuration;

namespace View.Playback.Commands
{
    public sealed record PauseForAreaEffect : ViewCommand
    {
        internal override float ApplyAndGetBlockSeconds(PlaybackContext context)
        {
            return Settings.GlobalSettings.EffectDisplayTime.CurrentValue / 1000f;
        }
    }
}
