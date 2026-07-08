#nullable enable
using System.Collections.Generic;
using View.Playback.Values;

namespace View.Playback.Commands
{
    public sealed record SetTileVisibilities(IReadOnlyList<PositionedTileVisibility> Tiles) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Tiles.SetVisibilities(Tiles);
        }
    }
}
