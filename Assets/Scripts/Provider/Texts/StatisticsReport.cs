#nullable enable
using System.Text;
using Game.Statistics;

namespace Provider.Texts
{
    internal sealed class StatisticsReport
    {
        private readonly WorldStatisticsSummary? _active;
        private readonly GlobalStatisticsSummary _global;

        public StatisticsReport(WorldStatisticsSummary? active, GlobalStatisticsSummary global)
        {
            _active = active;
            _global = global;
        }

        public string Text
        {
            get
            {
                var sb = new StringBuilder();
                if (_active != null)
                    sb.AppendLine(new WorldStatisticsText(_active).Text);
                sb.AppendLine(new GlobalStatisticsText(_global).Text);
                return sb.ToString();
            }
        }
    }
}
