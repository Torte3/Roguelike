#nullable enable
using Domain.Model.Character;

namespace Domain.Model.WorldEvents
{
    public record CharacterEntityLabel(CharacterLabel Character) : EntityLabel;
}
