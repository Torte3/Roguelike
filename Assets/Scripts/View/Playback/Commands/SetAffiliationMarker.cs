#nullable enable
using System;
using View.Playback.Values;

namespace View.Playback.Commands
{
    public sealed record SetAffiliationMarker(Guid Key, AffiliationMarker Marker) : InstantCommand
    {
        private protected override void Run(PlaybackContext context)
        {
            context.Entities.SetAffiliation(Key, Marker);
        }
    }
}
