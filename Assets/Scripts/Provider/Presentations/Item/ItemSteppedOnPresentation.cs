#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Item
{
    internal sealed class ItemSteppedOnPresentation : EntityPresentation<ItemSteppedOn>
    {
        protected override IEnumerable<ViewOp> OpsOf(ItemSteppedOn worldEvent)
        {
            if (worldEvent.IsVisible)
                yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{Names.Of(worldEvent.ItemName).Paint(Tint.Notice)}の上に乗った");
        }
    }
}
