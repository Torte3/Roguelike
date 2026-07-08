#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Combat
{
    internal sealed class ConditionRemovedPresentation : EntityPresentation<ConditionRemoved>
    {
        protected override IEnumerable<ViewOp> OpsOf(ConditionRemoved worldEvent)
        {
            foreach (var op in ShopPrices.Of(worldEvent.Shop))
                yield return op;
            yield return new SetParticles(worldEvent.Entity.Key(), worldEvent.Particles);
            if (worldEvent.IsVisible && worldEvent.ConditionLog != null)
                yield return Logs.Line($"{Names.Of(worldEvent.Label)}{worldEvent.ConditionLog}");
        }
    }
}
