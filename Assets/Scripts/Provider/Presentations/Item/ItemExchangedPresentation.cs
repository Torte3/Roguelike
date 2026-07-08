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
    internal sealed class ItemExchangedPresentation : Presentation<ItemExchanged>
    {
        protected override Guid? SubjectOf(ItemExchanged worldEvent) => KeyOf(worldEvent.Entity);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemExchanged worldEvent)
        {
            var name = new CharacterNameText(worldEvent.Label).Text;
            yield return AddLog.Line($"{name}は{new ItemNameText(worldEvent.PickedUpName).Text}を拾った");
            yield return AddLog.Line($"{name}は{new ItemNameText(worldEvent.DroppedName).Text}を捨てた");
            yield return new PlaySE(SEKind.Pickup);
            yield return new PopupIcon(worldEvent.Obtained.Icon, worldEvent.Obtained.Position);
        }
    }
}
