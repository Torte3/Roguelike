#nullable enable
using System;
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemGivenPresentation : Presentation<ItemGiven>
    {
        protected override Guid? SubjectOf(ItemGiven worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemGiven worldEvent)
        {
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}に{new ItemNameText(worldEvent.ItemName).Text}を渡した。");
        }
    }
}
