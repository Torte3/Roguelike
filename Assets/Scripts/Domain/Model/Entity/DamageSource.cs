#nullable enable
using Domain.Model.Memento;

namespace Domain.Model.Entity
{
    public abstract record DamageSource
    {
        public abstract DamageSourceMemento Serialize();
    }
}
