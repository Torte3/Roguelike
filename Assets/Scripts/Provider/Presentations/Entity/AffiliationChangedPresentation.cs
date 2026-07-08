#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class AffiliationChangedPresentation : Presentation<AffiliationChanged>
    {
        private readonly AffiliationMarkers _markers;

        public AffiliationChangedPresentation(AffiliationMarkers markers)
        {
            _markers = markers;
        }

        protected override Guid? SubjectOf(AffiliationChanged worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(AffiliationChanged worldEvent)
        {
            var key = KeyOf(worldEvent.Entity);
            yield return new SetAffiliationMarker(key, _markers.Of(worldEvent.Affiliation));
            yield return new SetInteractable(key, worldEvent.AppearsInteractable);
        }
    }
}
