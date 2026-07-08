#nullable enable

namespace View.Playback.Commands
{
    public sealed record RebuildView : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Clock.Reset();
            context.Entities.Clear();
            context.DamageTexts.DeleteAllText();
        }
    }
}
