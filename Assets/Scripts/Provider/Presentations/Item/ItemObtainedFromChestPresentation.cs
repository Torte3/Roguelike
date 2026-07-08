#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemObtainedFromChestPresentation : Presentation<ItemObtainedFromChest>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(ItemObtainedFromChest worldEvent)
        {
            yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{new ItemNameText(worldEvent.ItemName).Text}を手に入れた");
            yield return new PopupIcon(worldEvent.Obtained.Icon, worldEvent.Obtained.Position);
        }
    }
}
