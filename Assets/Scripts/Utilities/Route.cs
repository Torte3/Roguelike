#nullable enable
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    public class Route
    {
        private readonly List<Vector2Int> _cells;

        internal Route(List<Vector2Int> cells)
        {
            _cells = cells;
        }

        public bool HasNoStep => _cells.Count < 2;
        public Vector2Int Start => _cells[0];
        public Vector2Int FirstStep => _cells[1];
        public Vector2Int Last => _cells[^1];
    }
}
