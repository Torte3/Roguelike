#nullable enable
namespace Domain.Model.Entity
{
    public record DamageSource(DamageCause Cause, Opponent? Opponent = null, CollisionRole Collision = CollisionRole.None);
}
