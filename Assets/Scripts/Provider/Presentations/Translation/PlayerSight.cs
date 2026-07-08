#nullable enable
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Provider.Presentations.Translation
{
    public sealed class PlayerSight
    {
        private HashSet<Vector2Int> _area = new();

        internal void MapEntered(IReadOnlyCollection<Vector2Int> visibleArea)
        {
            _area = new HashSet<Vector2Int>(visibleArea);
        }

        internal IReadOnlyCollection<Vector2Int> Changed(IReadOnlyCollection<Vector2Int> visibleArea)
        {
            var changed = new HashSet<Vector2Int>(_area);
            _area = new HashSet<Vector2Int>(visibleArea);
            changed.SymmetricExceptWith(_area);
            return changed;
        }

        internal bool Contains(Vector2Int position) => _area.Contains(position);

        internal IReadOnlyList<Vector2Int> VisibleOf(IEnumerable<Vector2Int> positions)
        {
            return positions.Where(_area.Contains).ToList();
        }
    }
}
