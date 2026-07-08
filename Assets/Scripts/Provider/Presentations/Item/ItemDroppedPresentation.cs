#nullable enable
using System;
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemDroppedPresentation : Presentation<ItemDropped>
    {
        protected override Guid? SubjectOf(ItemDropped worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemDropped worldEvent)
        {
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{new ItemNameText(worldEvent.ItemName).Text}を落とした");
        }
    }
}
