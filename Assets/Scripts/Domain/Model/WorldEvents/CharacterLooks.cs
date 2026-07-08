#nullable enable
namespace Domain.Model.WorldEvents
{
    public record CharacterLooks(string TypeName, string SubtypeName, bool IsBoss, bool IsFlying);
}
