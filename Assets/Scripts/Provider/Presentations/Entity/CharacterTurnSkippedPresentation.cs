#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterTurnSkippedPresentation : EntityPresentation<CharacterTurnSkipped>
    {
        private const float PlayerPauseSeconds = 0.2f;

        protected override bool WaitsForMovers(CharacterTurnSkipped worldEvent)
        {
            return worldEvent.Label.IsPlayer;
        }

        protected override IEnumerable<ViewOp> OpsOf(CharacterTurnSkipped worldEvent)
        {
            if (worldEvent.Label.IsPlayer)
                yield return new Pause(PlayerPauseSeconds);
        }
    }
}
