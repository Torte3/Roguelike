#nullable enable
using System.Linq;
using System.Text;
using Game.Statistics;

namespace Provider.Texts
{
    internal sealed class GlobalStatisticsText : StatisticsText
    {
        private readonly GlobalStatisticsSummary _summary;

        public GlobalStatisticsText(GlobalStatisticsSummary summary)
        {
            _summary = summary;
        }

        public override string Text
        {
            get
            {
                var common = _summary.Common;
                var sb = new StringBuilder();
                sb.AppendLine("=== Global Statistics ===");
                AppendProgress(sb, common, $"TotalPlayTime: {common.PlayTime}", $"TotalTurns: {common.Turns}");
                AppendCombatAndEvents(sb, common, $"通算盗み回数: {common.StealCount}",
                    $"通算進入回数: {common.MonsterHouseEnterCount}");
                AppendItemUses(sb, common, "--- アイテム ---");
                sb.AppendLine($"KnownItemNames Count: {_summary.KnownItemNames.Count}");
                foreach (var itemName in _summary.KnownItemNames.OrderBy(x => x))
                    sb.AppendLine($"  {itemName}");
                AppendDeaths(sb, common);
                return sb.ToString();
            }
        }
    }
}
