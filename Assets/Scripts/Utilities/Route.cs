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
            FirstDirection = IsArrived ? null : DirectionMethods.FromVector(FirstStep - cells[0]);
        }

        public bool IsArrived => _cells.Count < 2;
        public Vector2Int FirstStep => _cells[1];
        public Direction8? FirstDirection { get; }
        public Vector2Int Last => _cells[^1];
    }
}
