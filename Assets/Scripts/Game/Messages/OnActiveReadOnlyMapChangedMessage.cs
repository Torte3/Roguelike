#nullable enable
using Domain.Model.Map;

namespace Game.Messages
{
    public record OnActiveReadOnlyMapChangedMessage(IReadOnlyMap Map);
}
