#nullable enable

using View.Playback.Values;

namespace View.Playback.Commands
{
    public sealed record PlaySE(SEKind Kind) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.SEManager.Play(Kind);
        }
    }
}
