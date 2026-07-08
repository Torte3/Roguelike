#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemsMergedPresentation : Presentation<ItemsMerged>
    {
        protected override IEnumerable<ViewCommand> CommandsOf(ItemsMerged worldEvent)
        {
            yield return AddLog.Line(
                $"{new CharacterNameText(worldEvent.Label).Text}は{new ItemNameText(worldEvent.BaseItemName).Text}と{new ItemNameText(worldEvent.MergedItemName).Text}を合成した。");
        }
    }
}
