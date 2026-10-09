#nullable enable
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations
{
    internal static class HealthOps
    {
        public static IEnumerable<ViewOp> Of(IHealthEvent worldEvent)
        {
            yield return new SetHpBar(worldEvent.Entity.Key(), worldEvent.Health.Hp, worldEvent.Health.MaxHp);
            foreach (var op in PlayerStatus(worldEvent))
                yield return op;
        }

        public static IEnumerable<ViewOp> PlayerStatus(IHealthEvent worldEvent)
        {
            if (!worldEvent.Label.IsPlayer)
                yield break;

            yield return new SetStatusHp(worldEvent.Health.Hp, worldEvent.Health.MaxHp);
        }
    }
}
