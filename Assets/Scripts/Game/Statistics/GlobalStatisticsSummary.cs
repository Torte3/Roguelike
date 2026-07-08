#nullable enable
using System.Collections.Generic;

namespace Game.Statistics
{
    public record GlobalStatisticsSummary(StatisticsSummary Common, IReadOnlyCollection<string> KnownItemNames);
}
