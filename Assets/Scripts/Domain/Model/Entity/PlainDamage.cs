#nullable enable
using Domain.Model.Memento;

namespace Domain.Model.Entity
{
    public sealed record PlainDamage(DamageCause Cause, Opponent? Opponent = null) : DamageSource
    {
        public override DamageSourceMemento Serialize() => DamageSourceMemento.Plain(Cause, Opponent);
    }
}
