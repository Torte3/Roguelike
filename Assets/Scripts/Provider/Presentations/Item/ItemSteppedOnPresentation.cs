#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemSteppedOnPresentation : Presentation<ItemSteppedOn>
    {
        protected override Guid? SubjectOf(ItemSteppedOn worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemSteppedOn worldEvent)
        {
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{Tint.Notice.Paint(new ItemNameText(worldEvent.ItemName).Text)}の上に乗った");
        }
    }
}
