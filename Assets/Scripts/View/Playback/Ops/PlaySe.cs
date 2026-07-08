#nullable enable

namespace View.Playback.Ops
{
    public sealed record PlaySe(SeKind Kind) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Sounds.Play(Kind);
        }
    }
}
