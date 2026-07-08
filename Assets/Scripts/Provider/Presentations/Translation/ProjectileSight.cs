#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;
using UnityEngine;
using Utilities;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Translation
{
    public sealed class ProjectileSight
    {
        private readonly PlayerSight _sight;

        public ProjectileSight(PlayerSight sight)
        {
            _sight = sight;
        }

        internal IReadOnlyList<ViewCommand> Flew(Flight flight)
        {
            var path = _sight.VisibleOf(PathOf(flight.From, flight.To));
            if (path.Count < 2)
                return Array.Empty<ViewCommand>();
            return new ViewCommand[] { new FlyProjectile(flight.Icon, path.First(), path.Last()) };
        }

        private static IEnumerable<Vector2Int> PathOf(Vector2Int from, Vector2Int to)
        {
            var cells = Mathf.RoundToInt(VectorExtension.ChebyshevDistance(from, to));
            var step = new Vector2Int(Math.Sign(to.x - from.x), Math.Sign(to.y - from.y));
            return Enumerable.Range(0, cells + 1).Select(index => from + step * index);
        }
    }
}
