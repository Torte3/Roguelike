#nullable enable

namespace View.Playback.Commands
{
    public sealed record SetDungeonInfo(string Text) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.DungeonInfo.SetInfo(Text);
        }
    }
}
