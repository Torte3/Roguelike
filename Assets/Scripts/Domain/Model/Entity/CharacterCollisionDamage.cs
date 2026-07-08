#nullable enable
using Domain.Model.Memento;

namespace Domain.Model.Entity
{
    public sealed record CharacterCollisionDamage(Opponent Opponent, CollisionSide Side) : DamageSource
    {
        public override DamageSourceMemento Serialize() => DamageSourceMemento.CharacterCollision(Opponent, Side);
    }
}
