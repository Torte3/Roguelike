#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using View.Playback;

namespace Provider.Presentations.Entity
{
    internal sealed class ChargeEndedPresentation : Presentation<ChargeEnded>
    {
        private readonly ChargePreviews _charges;

        public ChargeEndedPresentation(ChargePreviews charges)
        {
            _charges = charges;
        }

        protected override Guid? SubjectOf(ChargeEnded worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> CommandsOf(ChargeEnded worldEvent)
        {
            yield return _charges.Ended(KeyOf(worldEvent.Entity));
        }
    }
}
