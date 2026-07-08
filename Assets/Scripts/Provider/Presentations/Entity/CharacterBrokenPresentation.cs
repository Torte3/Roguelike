#nullable enable
using System;
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterBrokenPresentation : Presentation<CharacterBroken>
    {
        protected override Guid? SubjectOf(CharacterBroken worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(CharacterBroken worldEvent)
        {
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は破壊された");
        }
    }
}
