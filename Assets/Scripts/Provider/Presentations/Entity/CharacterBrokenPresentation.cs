#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterBrokenPresentation : EntityPresentation<CharacterBroken>
    {
        protected override IEnumerable<ViewOp> OpsOf(CharacterBroken worldEvent)
        {
            foreach (var op in ShopPrices.Of(worldEvent.Shop))
                yield return op;
            if (worldEvent.IsVisible)
                yield return Logs.Line($"{Names.Of(worldEvent.Label)}は破壊された");
        }
    }
}
