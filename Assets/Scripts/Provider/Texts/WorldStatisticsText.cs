#nullable enable
using System.Text;
using Game.Statistics;

namespace Provider.Texts
{
    internal sealed class WorldStatisticsText : StatisticsText
    {
        private readonly WorldStatisticsSummary _summary;

        public WorldStatisticsText(WorldStatisticsSummary summary)
        {
            _summary = summary;
        }

        public override string Text
        {
            get
            {
                var common = _summary.Common;
                var sb = new StringBuilder();
                sb.AppendLine("=== World Statistics ===");
                AppendProgress(sb, common, $"PlayTime: {common.PlayTime}", $"Turn: {common.Turns}");
                sb.AppendLine($"IsCheating: {_summary.IsCheating}");
                AppendCombatAndEvents(sb, common, $"盗み回数: {common.StealCount}", $"進入回数: {common.MonsterHouseEnterCount}");
                AppendItemUses(sb, common, "--- アイテム使用 ---");
                AppendDeaths(sb, common);
                return sb.ToString();
            }
        }
    }
}
