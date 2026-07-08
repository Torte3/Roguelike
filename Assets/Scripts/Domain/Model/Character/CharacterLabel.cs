#nullable enable
using Domain.Model.Character.Affiliation;

namespace Domain.Model.Character
{
    public record CharacterLabel(string Name, AffiliationType Affiliation);
}
