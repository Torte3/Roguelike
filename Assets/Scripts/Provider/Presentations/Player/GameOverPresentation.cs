#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Player
{
    internal sealed class GameOverPresentation : Presentation<GameOver>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(GameOver worldEvent)
        {
            yield return new ShowGameOver($"到達階層: {worldEvent.MaxMapLevel}F", worldEvent.Score,
                new DeathText(worldEvent.Death.Victim, worldEvent.Death.Source).Text);
        }
    }
}
