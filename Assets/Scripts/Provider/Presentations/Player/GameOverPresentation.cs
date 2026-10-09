#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Player
{
    internal sealed class GameOverPresentation : Presentation<GameOver>
    {
        protected override IEnumerable<ViewOp> OpsOf(GameOver worldEvent)
        {
            yield return new ShowGameOver(worldEvent.MaxMapLevel, worldEvent.Score,
                DeathText.Of(worldEvent.Death.Victim, worldEvent.Death.Source));
        }
    }
}
