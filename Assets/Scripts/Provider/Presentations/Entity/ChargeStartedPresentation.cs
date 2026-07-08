#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;

namespace Provider.Presentations.Entity
{
    internal sealed class ChargeStartedPresentation : Presentation<ChargeStarted>
    {
        private readonly ChargePreviews _charges;

        public ChargeStartedPresentation(ChargePreviews charges)
        {
            _charges = charges;
        }

        protected override Guid? SubjectOf(ChargeStarted worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(ChargeStarted worldEvent)
        {
            yield return _charges.Started(KeyOf(worldEvent.Entity), worldEvent.Charge);
        }
    }
}
