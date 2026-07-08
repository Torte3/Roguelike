#nullable enable
using System.Linq;
using System.Text;
using Game;
using Provider.Texts;

namespace Provider
{
    internal static class StatisticsText
    {
        public static string Of(WorldStatisticsSummary? active, GlobalStatisticsSummary global)
        {
            var sb = new StringBuilder();
            if (active != null)
                sb.AppendLine(Of(active));
            sb.AppendLine(Of(global));
            return sb.ToString();
        }

        public static string Of(WorldStatisticsSummary summary)
        {
            var common = summary.Common;
            var sb = new StringBuilder();
            sb.AppendLine("=== World Statistics ===");
            AppendProgress(sb, common, $"PlayTime: {common.PlayTime}", $"Turn: {common.Turns}");
            sb.AppendLine($"IsCheating: {summary.IsCheating}");
            AppendCombatAndEvents(sb, common, $"盗み回数: {common.StealCount}", $"進入回数: {common.MonsterHouseEnterCount}");
            AppendItemUses(sb, common, "--- アイテム使用 ---");
            AppendDeaths(sb, common);
            return sb.ToString();
        }

        public static string Of(GlobalStatisticsSummary summary)
        {
            var common = summary.Common;
            var sb = new StringBuilder();
            sb.AppendLine("=== Global Statistics ===");
            AppendProgress(sb, common, $"TotalPlayTime: {common.PlayTime}", $"TotalTurns: {common.Turns}");
            AppendCombatAndEvents(sb, common, $"通算盗み回数: {common.StealCount}",
                $"通算進入回数: {common.MonsterHouseEnterCount}");
            AppendItemUses(sb, common, "--- アイテム ---");
            sb.AppendLine($"KnownItemNames Count: {summary.KnownItemNames.Count}");
            foreach (var itemName in summary.KnownItemNames.OrderBy(x => x))
                sb.AppendLine($"  {itemName}");
            AppendDeaths(sb, common);
            return sb.ToString();
        }

        private static void AppendProgress(StringBuilder sb, StatisticsSummary summary, string playTimeLine,
            string turnsLine)
        {
            sb.AppendLine("--- プレイ・進行 ---");
            sb.AppendLine(playTimeLine);
            sb.AppendLine(turnsLine);
            sb.AppendLine($"MaxMapLevel: {summary.MaxMapLevel}");
        }

        private static void AppendCombatAndEvents(StringBuilder sb, StatisticsSummary summary, string stealLine,
            string monsterHouseLine)
        {
            sb.AppendLine("--- 戦闘・ダメージ ---");
            sb.AppendLine($"TotalDamageReceived: {summary.DamageReceived} (Max: {summary.MaxDamageReceived})");
            sb.AppendLine($"TotalDamageDealt: {summary.DamageDealt} (Max: {summary.MaxDamageDealt})");
            sb.AppendLine($"TotalHealReceived: {summary.HealReceived} (Max: {summary.MaxHealReceived})");
            sb.AppendLine("--- 盗み ---");
            sb.AppendLine(stealLine);
            sb.AppendLine("--- モンスターハウス ---");
            sb.AppendLine(monsterHouseLine);
            sb.AppendLine("--- 呪い ---");
            sb.AppendLine($"呪われたアイテムを発見した回数: {summary.CursedItemDiscoverCount}");
            sb.AppendLine("--- 敵撃破 ---");
            sb.AppendLine($"EnemyKilledCount: {summary.EnemyKilledCounts.Values.Sum()}");
            foreach (var kvp in summary.EnemyKilledCounts.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
        }

        private static void AppendItemUses(StringBuilder sb, StatisticsSummary summary, string header)
        {
            sb.AppendLine(header);
            sb.AppendLine($"TotalItemUsedCount: {summary.ItemUsedCounts.Values.Sum()}");
            foreach (var kvp in summary.ItemUsedCounts.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
        }

        private static void AppendDeaths(StringBuilder sb, StatisticsSummary summary)
        {
            sb.AppendLine("--- 死亡 ---");
            sb.AppendLine($"TotalDeathCount: {summary.DeathCounts.Values.Sum()}");
            var countsByText = summary.DeathCounts
                .GroupBy(kvp => DeathText.Of(kvp.Key.Victim, kvp.Key.Source), kvp => kvp.Value)
                .Select(group => (Text: group.Key, Count: group.Sum()));
            foreach (var (text, count) in countsByText.OrderByDescending(x => x.Count))
                sb.AppendLine($"  {text}: {count}");
        }
    }
}
