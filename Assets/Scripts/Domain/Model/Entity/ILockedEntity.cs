#nullable enable
using System.Collections.Generic;
using Utilities;

namespace Domain.Model.Entity
{
    public interface ILockedEntity : IEntity
    {
        public List<Id<IEntity>> KeyCharacters { get; }
    }
}
