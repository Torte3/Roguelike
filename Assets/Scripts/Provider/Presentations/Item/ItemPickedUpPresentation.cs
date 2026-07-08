#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Item
{
    internal sealed class ItemPickedUpPresentation : Presentation<ItemPickedUp>
    {
        protected override Guid? SubjectOf(ItemPickedUp worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemPickedUp worldEvent)
        {
            var itemName = new ItemNameText(worldEvent.ItemName).Text;
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{(worldEvent.IsAutomatic ? Tint.Notice.Paint(itemName) : itemName)}を拾った");
            yield return new PlaySE(SEKind.Pickup);
            yield return new PopupIcon(worldEvent.Obtained.Icon, worldEvent.Obtained.Position);
        }
    }
}
