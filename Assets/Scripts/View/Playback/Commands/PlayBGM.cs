#nullable enable

using View.Playback.Values;

namespace View.Playback.Commands
{
    public sealed record PlayBGM(BGMTrack Track) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.BGMManager.Play(Track);
        }
    }
}
