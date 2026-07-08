#nullable enable

namespace View.Playback.Commands
{
    public sealed record AddLog(string Text, bool AppendsToPrevious) : InstantCommand
    {
        public static AddLog Line(string text) => new(text, false);

        public static AddLog Appended(string text) => new(text, true);

        private protected override void Run(PlaybackContext context)
        {
            context.Log.AddLog(Text, AppendsToPrevious);
        }
    }
}
