using Domain.Model.Entity;
using Domain.Model.Map;
using Utilities;

namespace Game
{
    public record MovementData(MovementEntityType Type, Id<IMap> Destination, Id<IEntity>? Id,
        Id<IEntity>? DestinationId);
}