#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Item
{
    internal sealed class ItemBrokenPresentation : Presentation<ItemBroken>
    {
        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemBroken worldEvent)
        {
            yield return AddLog.Line($"{new ItemNameText(worldEvent.ItemName).Text}は壊れた");
            yield return new PlaySE(SEKind.ItemBreak);
        }
    }
}
