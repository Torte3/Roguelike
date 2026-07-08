#nullable enable
using Domain.Model.WorldEvents;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemIdentifiedPresentation : Presentation<ItemIdentified>
    {
        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemIdentified worldEvent)
        {
            if (worldEvent.Cause != DiscoveryCause.Automatic)
                yield return AddLog.Line(
                    $"{new ItemNameText(worldEvent.NameBefore).Text}は{new ItemNameText(worldEvent.NameBefore with { IsIdentified = true }).Text}だった");
        }
    }
}
