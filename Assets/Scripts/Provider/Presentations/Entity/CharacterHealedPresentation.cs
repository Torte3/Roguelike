#nullable enable
using System;
using Domain.Model.Entity;
using Domain.Model.WorldEvents;
using Provider.Presentations.Translation;
using Provider.Texts;
using System.Collections.Generic;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Entity
{
    internal sealed class CharacterHealedPresentation : Presentation<CharacterHealed>
    {
        private readonly HealthCommands _healthCommands;

        public CharacterHealedPresentation(HealthCommands healthCommands)
        {
            _healthCommands = healthCommands;
        }

        protected override Guid? SubjectOf(CharacterHealed worldEvent) => KeyOf(worldEvent.Entity);

        protected override bool IsWaitingForMovers(CharacterHealed worldEvent)
        {
            return worldEvent.IsVisible && worldEvent.Cause != HealCause.NaturalRecovery;
        }

        protected override IEnumerable<ViewCommand> CommandsOf(CharacterHealed worldEvent) =>
            _healthCommands.HpBarAndPlayerStatus(worldEvent.Entity, worldEvent.Health);

        protected override IEnumerable<ViewCommand> VisibleCommandsOf(CharacterHealed worldEvent)
        {
            if (worldEvent.Cause == HealCause.Effect)
                yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{worldEvent.Amount}回復");
            else if (worldEvent.Cause == HealCause.KillHeal && worldEvent.Restored > 0)
                yield return AddLog.Line($"{new CharacterNameText(worldEvent.Label).Text}は{worldEvent.Restored}回復");
            yield return _healthCommands.Healed(worldEvent);
        }
    }
}
