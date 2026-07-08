#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemBrokenPresentation : Presentation<ItemBroken>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemBroken worldEvent)
        {
            if (!worldEvent.IsVisible)
                yield break;

            yield return Logs.Line($"{Names.Of(worldEvent.ItemName)}は壊れた");
            yield return new PlaySe(SeKind.ItemBreak);
        }
    }
}
