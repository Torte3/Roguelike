using Domain.Model.Entity;
using Utilities;

namespace Domain.Model.Map
{
    public interface IMovementEntity : IEntity
    {
        MovementEntityType Type { get; }
        Id<IMap> Destination { get; }
    }
}