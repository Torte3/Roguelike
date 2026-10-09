#nullable enable
using System.Collections.Generic;

namespace Game
{
    public record GlobalStatisticsSummary(StatisticsSummary Common, IReadOnlyCollection<string> KnownItemNames);
}
