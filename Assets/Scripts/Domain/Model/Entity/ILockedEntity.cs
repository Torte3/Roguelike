#nullable enable
using Utilities;

namespace Domain.Model.Entity
{
    public interface ILockedEntity : IEntity
    {
        public bool IsLockReleased { get; }
        public bool IsKeyHolder(Id<IEntity> id);
        public void ForgetKeyHolder(Id<IEntity> id);
    }
}
