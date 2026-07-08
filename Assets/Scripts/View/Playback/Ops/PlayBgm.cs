#nullable enable

namespace View.Playback.Ops
{
    public sealed record PlayBgm(BgmTrack Track) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Music.Play(Track);
        }
    }
}
