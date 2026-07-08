#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Item
{
    internal sealed class ItemCurseIdentifiedPresentation : Presentation<ItemCurseIdentified>
    {
        protected override IEnumerable<ViewCommand> VisibleCommandsOf(ItemCurseIdentified worldEvent)
        {
            if (worldEvent.IsCursed && IsAnnounced(worldEvent.Cause))
                yield return AddLog.Line($"{new ItemNameText(worldEvent.ItemName).Text}は呪われていた");
        }

        private static bool IsAnnounced(DiscoveryCause cause)
        {
            return cause switch
            {
                DiscoveryCause.Used or DiscoveryCause.Thrown or DiscoveryCause.Scroll => true,
                DiscoveryCause.Automatic or DiscoveryCause.CurseChanged => false,
                _ => throw new System.ArgumentOutOfRangeException(nameof(cause), cause, null),
            };
        }
    }
}
