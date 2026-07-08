#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;

namespace Provider.Presentations.Entity
{
    internal sealed class ChargeCountedDownPresentation : Presentation<ChargeCountedDown>
    {
        private readonly ChargePreviews _charges;

        public ChargeCountedDownPresentation(ChargePreviews charges)
        {
            _charges = charges;
        }

        protected override Guid? SubjectOf(ChargeCountedDown worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(ChargeCountedDown worldEvent)
        {
            return _charges.CountedDown(KeyOf(worldEvent.Entity), worldEvent.Turns);
        }
    }
}
