#nullable enable
using Domain.Model.Map;

namespace Game
{
    public record OnActiveReadOnlyMapChangedMessage(IReadOnlyMap Map, bool IsNewWorld);
}
