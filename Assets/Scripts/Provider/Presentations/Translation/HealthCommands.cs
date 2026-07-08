#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Configuration;
using Domain.Model.Character;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;

namespace Provider.Presentations.Translation
{
    public sealed class HealthCommands
    {
        private readonly GlobalSettings _settings;
        private readonly PlayerEntity _player;

        public HealthCommands(GlobalSettings settings, PlayerEntity player)
        {
            _settings = settings;
            _player = player;
        }

        internal IReadOnlyList<ViewCommand> HpBarAndPlayerStatus(EntityRef entity, Health health)
        {
            return new ViewCommand[] { new SetHpBar(entity.Id.Value, health.Hp, health.MaxHp) }
                .Concat(PlayerStatus(entity, health))
                .ToList();
        }

        internal IReadOnlyList<ViewCommand> PlayerStatus(EntityRef entity, Health health)
        {
            if (!_player.Is(entity))
                return Array.Empty<ViewCommand>();

            return new ViewCommand[] { new SetStatusHp(health.Hp, health.MaxHp, IsLow(health)) };
        }

        internal IReadOnlyList<ViewCommand> Damaged(CharacterDamaged worldEvent)
        {
            var percentOfMaxHp = PercentOfMaxHp(worldEvent.Amount, worldEvent.Health);
            var damageNumber = new ShowDamageNumber(worldEvent.Entity.Position, worldEvent.Amount, FontSizeOf(percentOfMaxHp));
            if (_player.Is(worldEvent.Entity) && (IsSignificant(percentOfMaxHp) || IsLow(worldEvent.Health)))
                return new ViewCommand[] { new Flash(), damageNumber };
            return new ViewCommand[] { damageNumber };
        }

        internal ViewCommand Healed(CharacterHealed worldEvent)
        {
            return new ShowHealNumber(worldEvent.Entity.Position, worldEvent.Amount,
                FontSizeOf(PercentOfMaxHp(worldEvent.Amount, worldEvent.Health)));
        }

        private static int PercentOfMaxHp(int amount, Health health) => amount * 100 / health.MaxHp;

        private bool IsSignificant(int percentOfMaxHp) =>
            percentOfMaxHp > _settings.SignificantDamageThresholdPercentage.CurrentValue;

        private bool IsLow(Health health) =>
            PercentOfMaxHp(health.Hp, health) < _settings.LowHpThresholdPercentage.CurrentValue;

        private static float FontSizeOf(int percentOfMaxHp)
        {
            if (percentOfMaxHp > 100)
                return 1f;
            if (percentOfMaxHp > 25)
                return 0.8f;
            if (percentOfMaxHp > 10)
                return 0.7f;
            return 0.5f;
        }
    }
}
