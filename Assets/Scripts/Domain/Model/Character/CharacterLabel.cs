#nullable enable
namespace Domain.Model.Character
{
    public record CharacterLabel(string Name, bool IsPlayer, AffiliationType Affiliation);
}
