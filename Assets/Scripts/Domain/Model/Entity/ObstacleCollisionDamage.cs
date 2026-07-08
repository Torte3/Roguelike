#nullable enable
using Domain.Model.Memento;

namespace Domain.Model.Entity
{
    public sealed record ObstacleCollisionDamage(CollisionObstacle Obstacle) : DamageSource
    {
        public override DamageSourceMemento Serialize() => DamageSourceMemento.ObstacleCollision(Obstacle);
    }
}
