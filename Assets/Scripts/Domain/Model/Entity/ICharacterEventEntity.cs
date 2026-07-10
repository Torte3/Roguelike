#nullable enable
using Domain.Model;

namespace Domain.Model.Entity
{
    public interface ICharacterEventEntity : IEntity
    {
        public ICharacterEvent Event { get; }
    }
}
