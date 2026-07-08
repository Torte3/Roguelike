#nullable enable

namespace View.Playback.Ops
{
    public sealed record AddLog(string Text, bool AppendsToPrevious) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Log.AddLog(Text, AppendsToPrevious);
        }
    }
}
