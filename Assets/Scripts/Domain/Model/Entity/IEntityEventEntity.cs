#nullable enable
using Domain.Model;

namespace Domain.Model.Entity
{
    public interface IEntityEventEntity : IEntity
    {
        public IEntityEvent Event { get; }
    }
}
