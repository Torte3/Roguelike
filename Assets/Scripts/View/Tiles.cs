using System;
using UnityEngine.Tilemaps;

namespace View
{
    [Serializable]
    internal struct Tiles
    {
        public TileBase Floor;
        public TileBase ShopFloor;
        public TileBase Water;
        public TileBase Wall;
    }
    [Serializable]
    internal struct WorldTiles
    {
        public TileBase Grass;
        public TileBase Ocean;
        public TileBase Mountain;
        public TileBase Desert;
        public TileBase Forest;
    }
}