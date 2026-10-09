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
}