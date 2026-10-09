#nullable enable
using Domain.Model.Character;

namespace Domain.Model.Entity
{
    public record DeathRecord(CharacterLabel Victim, DamageSource Source);
}
