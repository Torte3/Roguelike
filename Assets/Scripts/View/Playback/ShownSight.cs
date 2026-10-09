#nullable enable
using System.Collections.Generic;
using R3;
using UnityEngine;

namespace View.Playback
{
    public sealed class ShownSight
    {
        private readonly Subject<IReadOnlyCollection<Vector2Int>> _onChanged = new();
        private HashSet<Vector2Int> _area = new();

        internal Observable<IReadOnlyCollection<Vector2Int>> OnChanged => _onChanged;

        internal bool Contains(Vector2Int position)
        {
            return _area.Contains(position);
        }

        internal void Reset(IEnumerable<Vector2Int> area)
        {
            _area = new HashSet<Vector2Int>(area);
        }

        internal void Change(IEnumerable<Vector2Int> area)
        {
            var changed = new HashSet<Vector2Int>(_area);
            _area = new HashSet<Vector2Int>(area);
            changed.SymmetricExceptWith(_area);
            _onChanged.OnNext(changed);
        }
    }
}
