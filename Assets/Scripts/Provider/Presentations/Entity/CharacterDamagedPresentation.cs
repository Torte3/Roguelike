#nullable enable
using System.Collections.Generic;
using Domain.Model.Entity;
using Domain.Model.WorldEvents;
using Provider.Texts;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterDamagedPresentation : EntityPresentation<CharacterDamaged>
    {
        protected override bool WaitsForMovers(CharacterDamaged worldEvent)
        {
            return worldEvent.IsVisible && worldEvent.Source.Cause is not (DamageCause.Poison or DamageCause.Fire);
        }

        protected override IEnumerable<ViewOp> OpsOf(CharacterDamaged worldEvent)
        {
            foreach (var op in HealthOps.Of(worldEvent))
                yield return op;
            if (!worldEvent.IsVisible)
                yield break;

            if (LogOf(worldEvent) is { } log)
                yield return log;

            var health = worldEvent.Health;
            var percentOfMaxHp = worldEvent.Amount * 100 / health.MaxHp;
            if (worldEvent.Label.IsPlayer)
                yield return new FlashOnPlayerDamage(percentOfMaxHp, health.Hp * 100 / health.MaxHp);
            yield return new ShowDamageNumber(worldEvent.Entity.Position, worldEvent.Amount, percentOfMaxHp);
        }

        private static ViewOp? LogOf(CharacterDamaged worldEvent)
        {
            var name = Names.Of(worldEvent.Label);
            return worldEvent.Source.Cause switch
            {
                DamageCause.CriticalAttack => Logs.Appended($"クリティカル！{name}に{worldEvent.Amount}のダメージ。".Paint(Tint.Alert)),
                DamageCause.Attack or DamageCause.Explosion => Logs.Appended($"{name}に{worldEvent.Amount}のダメージ。"),
                DamageCause.Fire => Logs.Line($"{name}は火に焼かれた"),
                _ => null,
            };
        }
    }
}
