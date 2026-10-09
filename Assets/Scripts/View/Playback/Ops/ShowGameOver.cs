#nullable enable

namespace View.Playback.Ops
{
    public sealed record ShowGameOver(int Level, float Score, string CauseOfDeath) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Menus.ShowGameOver(Level, Score, CauseOfDeath);
        }
    }
}
