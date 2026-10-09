#nullable enable

namespace View.Playback.Ops
{
    public sealed record SetDungeonInfo(string Name, int Depth) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.DungeonInfo.SetInfo(Name, Depth);
        }
    }
}
