#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace View.Playback.Ops
{
    public sealed record EnterTiles(TileSet DefaultTileSet, RectInt? ShopRect, IReadOnlyList<TileSpec> Tiles, IReadOnlyList<OverlaySpec> Overlays) : InstantOp
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Tiles.Enter(DefaultTileSet, ShopRect, Tiles, Overlays);
        }
    }
}
