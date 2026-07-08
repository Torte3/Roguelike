#nullable enable
using System.Collections.Generic;
using Domain.Model.Character.Affiliation;
using View.Playback.Values;

namespace Provider.Presentations.Translation
{
    public sealed class AffiliationMarkers
    {
        private static readonly IReadOnlyDictionary<AffiliationType, AffiliationMarker> Markers =
            new Dictionary<AffiliationType, AffiliationMarker>
            {
                { AffiliationType.Enemy, AffiliationMarker.Enemy },
                { AffiliationType.Ally, AffiliationMarker.Ally },
                { AffiliationType.Neutral, AffiliationMarker.Neutral },
            };

        internal AffiliationMarker Of(AffiliationType affiliation) => Markers[affiliation];
    }
}
