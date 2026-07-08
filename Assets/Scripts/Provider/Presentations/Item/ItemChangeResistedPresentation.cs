#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemChangeResistedPresentation : Presentation<ItemChangeResisted>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemChangeResisted worldEvent)
        {
            if (!worldEvent.IsVisible)
                yield break;

            var item = Names.Of(worldEvent.ItemName);
            yield return Logs.Line(worldEvent.Kind switch
            {
                ItemChangeKind.Cursed => $"{item}は呪われなかった",
                ItemChangeKind.Downgraded => $"{item}の強化は消えなかった",
                _ => throw new ArgumentOutOfRangeException(nameof(worldEvent), worldEvent.Kind, null),
            });
        }
    }
}
