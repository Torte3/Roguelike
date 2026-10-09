#nullable enable

namespace View.Playback.Ops
{
    public sealed record Pause(float Seconds) : ViewOp
    {
        internal override float Apply(PlaybackContext context)
        {
            return Seconds;
        }
    }
}
