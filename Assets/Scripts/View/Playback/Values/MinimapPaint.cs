#nullable enable
using UnityEngine;
using View.Map;

namespace View.Playback.Values
{
    public abstract record MinimapPaint
    {
        public static MinimapPaint Floor { get; } = new FloorPaint();
        public static MinimapPaint Water { get; } = new WaterPaint();
        public static MinimapPaint Wall { get; } = new WallPaint();
        public static MinimapPaint UnbreakableWall { get; } = new UnbreakableWallPaint();

        internal abstract void Draw(MinimapController minimap, Vector2Int position);

        private sealed record FloorPaint : MinimapPaint
        {
            internal override void Draw(MinimapController minimap, Vector2Int position) => minimap.SetFloor(position);
        }

        private sealed record WaterPaint : MinimapPaint
        {
            internal override void Draw(MinimapController minimap, Vector2Int position) => minimap.SetWater(position);
        }

        private sealed record WallPaint : MinimapPaint
        {
            internal override void Draw(MinimapController minimap, Vector2Int position) => minimap.SetWall(position);
        }

        private sealed record UnbreakableWallPaint : MinimapPaint
        {
            internal override void Draw(MinimapController minimap, Vector2Int position) =>
                minimap.SetUnbreakableWall(position);
        }
    }
}
