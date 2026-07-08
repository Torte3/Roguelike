#nullable enable

namespace View.Playback.Commands
{
    public sealed record ShowGameOver(string LevelText, float Score, string CauseOfDeath) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Menus.ShowGameOver(LevelText, Score, CauseOfDeath);
        }
    }
}
