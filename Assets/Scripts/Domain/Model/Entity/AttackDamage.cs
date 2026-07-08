#nullable enable
using Domain.Model.Memento;

namespace Domain.Model.Entity
{
    public sealed record AttackDamage(Opponent? Opponent, bool IsCritical) : DamageSource
    {
        public override DamageSourceMemento Serialize() => DamageSourceMemento.Attack(Opponent, IsCritical);
    }
}
