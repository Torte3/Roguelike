#nullable enable
using Configuration;

namespace View.Playback.Ops
{
    public sealed record PauseForAreaEffect : ViewOp
    {
        internal override float Apply(PlaybackContext context)
        {
            return Settings.GlobalSettings.EffectDisplayTime.CurrentValue / 1000f;
        }
    }
}
