#nullable enable

namespace View.Playback.Ops
{
    public sealed record ClearLog : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Log.Clear();
        }
    }
}
