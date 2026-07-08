#nullable enable
using System.Collections.Generic;

namespace View.Playback.Ops
{
    public sealed record ChangeTiles(IReadOnlyList<TileSpec> Tiles) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Tiles.Change(Tiles);
        }
    }
}
