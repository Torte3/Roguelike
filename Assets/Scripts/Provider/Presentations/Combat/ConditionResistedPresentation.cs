#nullable enable
using System;
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Combat
{
    internal sealed class ConditionResistedPresentation : Presentation<ConditionResisted>
    {
        protected override Guid? SubjectOf(ConditionResisted worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ConditionResisted worldEvent)
        {
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{worldEvent.ConditionName}に耐性がある");
        }
    }
}
