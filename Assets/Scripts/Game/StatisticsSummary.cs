#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.Entity;

namespace Game
{
    public record StatisticsSummary(
        TimeSpan PlayTime,
        int Turns,
        int MaxMapLevel,
        int DamageReceived,
        int MaxDamageReceived,
        int DamageDealt,
        int MaxDamageDealt,
        int HealReceived,
        int MaxHealReceived,
        int StealCount,
        int MonsterHouseEnterCount,
        int CursedItemDiscoverCount,
        IReadOnlyDictionary<string, int> EnemyKilledCounts,
        IReadOnlyDictionary<string, int> ItemUsedCounts,
        IReadOnlyDictionary<DeathRecord, int> DeathCounts);
}
