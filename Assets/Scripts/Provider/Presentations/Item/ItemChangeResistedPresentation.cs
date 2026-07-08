#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemChangeResistedPresentation : Presentation<ItemChangeResisted>
    {
        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemChangeResisted worldEvent)
        {
            var itemName = new ItemNameText(worldEvent.ItemName).Text;
            yield return AddLog.Line(worldEvent.Kind switch
            {
                ItemChangeKind.Cursed => $"{itemName}は呪われなかった",
                ItemChangeKind.Downgraded => $"{itemName}の強化は消えなかった",
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent.Kind), worldEvent.Kind, null),
            });
        }
    }
}
