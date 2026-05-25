#nullable enable
namespace Domain.Model.Character.Status
{
    public record OnDamageReceivedMessage(int Damage, string CauseOfDamageLog, ICharacter? Attacker);
}