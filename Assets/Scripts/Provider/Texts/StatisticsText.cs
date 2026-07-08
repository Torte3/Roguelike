#nullable enable
using System.Linq;
using System.Text;
using Game.Statistics;
using Provider.Texts;

namespace Provider.Texts
{
    internal abstract class StatisticsText
    {
        public abstract string Text { get; }

        protected void AppendProgress(StringBuilder sb, StatisticsSummary summary, string playTimeLine,
            string turnsLine)
        {
            sb.AppendLine("--- プレイ・進行 ---");
            sb.AppendLine(playTimeLine);
            sb.AppendLine(turnsLine);
            sb.AppendLine($"MaxMapLevel: {summary.MaxMapLevel}");
        }

        protected void AppendCombatAndEvents(StringBuilder sb, StatisticsSummary summary, string stealLine,
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

        protected void AppendItemUses(StringBuilder sb, StatisticsSummary summary, string header)
        {
            sb.AppendLine(header);
            sb.AppendLine($"TotalItemUsedCount: {summary.ItemUsedCounts.Values.Sum()}");
            foreach (var kvp in summary.ItemUsedCounts.OrderByDescending(x => x.Value))
                sb.AppendLine($"  {kvp.Key}: {kvp.Value}");
        }

        protected void AppendDeaths(StringBuilder sb, StatisticsSummary summary)
        {
            sb.AppendLine("--- 死亡 ---");
            sb.AppendLine($"TotalDeathCount: {summary.DeathCounts.Values.Sum()}");
            var countsByText = summary.DeathCounts
                .GroupBy(kvp => new DeathText(kvp.Key.Victim, kvp.Key.Source).Text, kvp => kvp.Value)
                .Select(group => (Text: group.Key, Count: group.Sum()));
            foreach (var (text, count) in countsByText.OrderByDescending(x => x.Count))
                sb.AppendLine($"  {text}: {count}");
        }
    }
}
