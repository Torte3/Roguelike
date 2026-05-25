using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using R3;
using UnityEngine;
using Utilities.Stats;

namespace Domain.Service.Characters
{
    internal class VisionRange : IVisionRange
    {
        private ReadOnlyReactiveProperty<Vector2Int> _position;
        private ReadOnlyReactiveProperty<float> _range;
        private readonly IFlagStat _clairvoyantFlags;
        private readonly IFlagStat _blindFlags;
        private readonly IFlagStat _narrowVisionFlags;
        private readonly Func<bool> _canThroughWalls;
        public bool IsClairvoyant => _clairvoyantFlags.CurrentValue;
        public bool IsBlind => _blindFlags.CurrentValue;
        private Subject<Unit> _onVisibleAreaChanged = new();
        private readonly IMap _map;

        public VisionRange(ReadOnlyReactiveProperty<Vector2Int> position, ReadOnlyReactiveProperty<float> range,
            IFlagStat clairvoyantFlags, IFlagStat blindFlags, IFlagStat narrowVisionFlags, Func<bool> canThroughWalls,
            IMap map)
        {
            _position = position;
            _range = range;
            _canThroughWalls = canThroughWalls;
            _clairvoyantFlags = clairvoyantFlags;
            _blindFlags = blindFlags;
            _narrowVisionFlags = narrowVisionFlags;
            Observable.Merge(
                _position.AsUnitObservable(),
                _range.AsUnitObservable(),
                _clairvoyantFlags.Value.AsUnitObservable(),
                _blindFlags.Value.AsUnitObservable(),
                _narrowVisionFlags.Value.AsUnitObservable()
            ).Subscribe(_ =>
            {
                ChangeVisibleArea();
            });
            _map = map;
        }

        public IReadOnlyCollection<Vector2Int> VisibleArea => Calc(_position.CurrentValue);
        public Observable<Unit> OnVisibleAreaChanged => _onVisibleAreaChanged;

        public void Refresh()
        {
            ChangeVisibleArea();
        }

        private void ChangeVisibleArea()
        {
            _onVisibleAreaChanged.OnNext(Unit.Default);
        }

        public bool IsVisible(Vector2Int position)
        {
            if (_canThroughWalls())
            {
                if (IsClairvoyant)
                    return true;
                var viewRadius = ResolveVisionRadius(_range.CurrentValue + 0.5f);
                return (position - _position.CurrentValue).sqrMagnitude <= viewRadius * viewRadius;
            }

            if (IsClairvoyant)
                return true;
            return _map.IsVisible(_position.CurrentValue, position, ResolveVisionRadius(_range.CurrentValue + 0.5f));
        }

        private HashSet<Vector2Int> Calc(Vector2Int position)
        {
            var range = _range.CurrentValue + 0.5f;
            if (_canThroughWalls())
            {
                if (IsClairvoyant)
                    return _map.GetAllPositions();
                var viewRadius = ResolveVisionRadius(range);
                return _map.GetAllPositions().Where(
                    pos => (position - pos).sqrMagnitude <= viewRadius * viewRadius).ToHashSet();
            }

            if (IsClairvoyant)
                return _map.GetFullVisibleArea();
            return _map.GetVisibleArea(position, ResolveVisionRadius(range));
        }

        private float ResolveVisionRadius(float normalRange)
        {
            if (IsBlind)
                return CommonSenseParameters.BlindVisionRadius;
            if (_narrowVisionFlags.CurrentValue)
                return CommonSenseParameters.NarrowVisionRadius;
            return normalRange;
        }
    }
}
