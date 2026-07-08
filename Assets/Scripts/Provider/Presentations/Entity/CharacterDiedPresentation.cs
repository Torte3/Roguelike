#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterDiedPresentation : Presentation<CharacterDied>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(CharacterDied worldEvent)
        {
            yield return new WaitForEntity(KeyOf(worldEvent.Entity));
        }
    }
}
