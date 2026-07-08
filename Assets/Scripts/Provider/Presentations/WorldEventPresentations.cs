#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;

namespace Provider.Presentations
{
    internal sealed class WorldEventPresentations
    {
        private readonly IReadOnlyDictionary<Type, IWorldEventPresentation> _presentations;

        public WorldEventPresentations(IEnumerable<IWorldEventPresentation> presentations)
        {
            _presentations = presentations.ToDictionary(presentation => presentation.EventType);
        }

        public IWorldEventPresentation? Find(WorldEvent worldEvent)
        {
            return _presentations.TryGetValue(worldEvent.GetType(), out var presentation) ? presentation : null;
        }
    }
}
