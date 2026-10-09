#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Effect;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Map
{
    internal sealed class AreaEffectAppliedPresentation : Presentation<AreaEffectApplied>
    {
        protected override bool WaitsForMovers(AreaEffectApplied worldEvent)
        {
            return worldEvent.IsVisible;
        }

        protected override IEnumerable<ViewOp> OpsOf(AreaEffectApplied worldEvent)
        {
            if (worldEvent.IsVisible)
                yield return new PauseForAreaEffect();
            yield return new SpawnAreaEffect(worldEvent.Area, worldEvent.Color);

            var impacts = worldEvent.Hits.Where(hit => hit.Target.IsVisible).Select(hit => hit.Impact).ToHashSet();
            if (impacts.Contains(Impact.Harmful))
                yield return new PlaySe(SeKind.Attack);
            if (impacts.Contains(Impact.Beneficial))
                yield return new PlaySe(SeKind.Heal);
        }
    }
}
