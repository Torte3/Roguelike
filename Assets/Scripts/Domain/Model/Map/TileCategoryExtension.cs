using System.ComponentModel;

namespace Domain.Model.Map
{
    public static class TileCategoryExtension
    {
        public static bool IsWalkable(this TileCategory tileCategory)
        {
            return tileCategory switch
            {
                TileCategory.Floor => true,
                TileCategory.Water => false,
                TileCategory.Wall => false,
                TileCategory.UnbreakableWall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static bool IsPassable(this TileCategory tileCategory)
        {
            return tileCategory switch
            {
                TileCategory.Floor => true,
                TileCategory.Water => true,
                TileCategory.Wall => false,
                TileCategory.UnbreakableWall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException()
            };
        }

        public static bool IsTransparent(this TileCategory tileCategory)
        {
            return tileCategory switch
            {
                TileCategory.Floor => true,
                TileCategory.Water => true,
                TileCategory.Wall => false,
                TileCategory.UnbreakableWall => false,
                TileCategory.Blank => false,
                _ => throw new InvalidEnumArgumentException()
            };
        }
    }
}