#nullable enable
using Configuration;

namespace View.Playback.Ops
{
    public sealed record PauseForTeleport : ViewOp
    {
        internal override float Apply(PlaybackContext context)
        {
            return Settings.GlobalSettings.MoveMilliseconds.CurrentValue / 1000f;
        }
    }
}
