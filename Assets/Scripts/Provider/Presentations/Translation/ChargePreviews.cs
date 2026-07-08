#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using UnityEngine;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Translation
{
    public sealed class ChargePreviews
    {
        private readonly Dictionary<Guid, ChargePreview> _charges = new();
        private readonly PlayerSight _sight;

        public ChargePreviews(PlayerSight sight)
        {
            _sight = sight;
        }

        internal void MapEntered()
        {
            _charges.Clear();
        }

        internal IReadOnlyList<ViewCommand> SightChanged(IReadOnlyCollection<Vector2Int> changed)
        {
            return _charges
                .Where(charge => charge.Value.Area.Any(changed.Contains))
                .Select(charge => new SetCharge(charge.Key, Clipped(charge.Value)))
                .ToList();
        }

        internal ChargePreview? CharacterAppeared(Guid key, ChargeState? charge)
        {
            return Changed(key, charge);
        }

        internal ViewCommand Started(Guid key, ChargeState? charge)
        {
            return new SetCharge(key, Changed(key, charge));
        }

        internal IReadOnlyList<ViewCommand> CountedDown(Guid key, int turns)
        {
            if (!_charges.TryGetValue(key, out var charge))
                return Array.Empty<ViewCommand>();

            var counted = charge with { Turns = turns };
            _charges[key] = counted;
            return new ViewCommand[] { new SetCharge(key, Clipped(counted)) };
        }

        internal ViewCommand Ended(Guid key)
        {
            return new SetCharge(key, Changed(key, null));
        }

        internal void EntityDisappeared(Guid key)
        {
            _charges.Remove(key);
        }

        private ChargePreview? Changed(Guid key, ChargeState? charge)
        {
            var preview = PreviewOf(charge);
            if (preview == null)
            {
                _charges.Remove(key);
                return null;
            }

            _charges[key] = preview;
            return Clipped(preview);
        }

        private static ChargePreview? PreviewOf(ChargeState? charge)
        {
            return charge == null ? null : new ChargePreview(charge.Area.Positions, charge.Area.Color, charge.Turns);
        }

        private ChargePreview Clipped(ChargePreview charge)
        {
            return charge with { Area = _sight.VisibleOf(charge.Area) };
        }
    }
}
