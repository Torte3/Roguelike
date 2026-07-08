#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Entity;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using Provider.Texts;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterDamagedPresentation : Presentation<CharacterDamaged>
    {
        private readonly HealthCommands _healthCommands;

        public CharacterDamagedPresentation(HealthCommands healthCommands)
        {
            _healthCommands = healthCommands;
        }

        protected override Guid? SubjectOf(CharacterDamaged worldEvent) => KeyOf(worldEvent.Entity);

        protected override bool IsWaitingForMovers(CharacterDamaged worldEvent)
        {
            return worldEvent.IsVisible && worldEvent.Source is not PlainDamage { Cause: DamageCause.Poison or DamageCause.Fire };
        }

        protected override IEnumerable<ViewCommand> CommandsOf(CharacterDamaged worldEvent) =>
            _healthCommands.HpBarAndPlayerStatus(worldEvent.Entity, worldEvent.Health);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(CharacterDamaged worldEvent)
        {
            if (LogOf(worldEvent) is { } log)
                yield return log;

            foreach (var command in _healthCommands.Damaged(worldEvent))
                yield return command;
        }

        private ViewCommand? LogOf(CharacterDamaged worldEvent)
        {
            var name = new CharacterNameText(worldEvent.Label).Text;
            return worldEvent.Source switch
            {
                AttackDamage { IsCritical: true } attack => AfterAttacker(attack.Opponent, Tint.Alert.Paint($"クリティカル！{name}に{worldEvent.Amount}のダメージ。")),
                AttackDamage attack => AfterAttacker(attack.Opponent, $"{name}に{worldEvent.Amount}のダメージ。"),
                PlainDamage { Cause: DamageCause.Explosion } explosion => AfterAttacker(explosion.Opponent, $"{name}に{worldEvent.Amount}のダメージ。"),
                PlainDamage { Cause: DamageCause.Fire } => AddLog.Line($"{name}は火に焼かれた"),
                _ => null,
            };
        }

        private static AddLog AfterAttacker(Opponent? attacker, string text) =>
            attacker is { IsVisible: true } ? AddLog.Appended(text) : AddLog.Line(text);
    }
}
