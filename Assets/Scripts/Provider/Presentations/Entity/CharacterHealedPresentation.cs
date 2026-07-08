#nullable enable
using System.Collections.Generic;
using Domain.Model.Entity;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterHealedPresentation : EntityPresentation<CharacterHealed>
    {
        protected override bool WaitsForMovers(CharacterHealed worldEvent)
        {
            return worldEvent.IsVisible && worldEvent.Cause != HealCause.NaturalRecovery;
        }

        protected override IEnumerable<ViewOp> OpsOf(CharacterHealed worldEvent)
        {
            foreach (var op in HealthOps.Of(worldEvent))
                yield return op;
            if (!worldEvent.IsVisible)
                yield break;

            if (worldEvent.Cause == HealCause.Effect)
                yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{worldEvent.Amount}回復");
            else if (worldEvent.Cause == HealCause.KillHeal && worldEvent.Restored > 0)
                yield return Logs.Line($"{Names.Of(worldEvent.Label)}は{worldEvent.Restored}回復");
            yield return new ShowHealNumber(worldEvent.Entity.Position, worldEvent.Amount,
                worldEvent.Amount * 100 / worldEvent.Health.MaxHp);
        }
    }
}
