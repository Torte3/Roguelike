#nullable enable
using Domain.Model.Character;

namespace Domain.Model.WorldEvents
{
    public record CharacterSkillSource(CharacterLabel Label, string Log) : SkillSource;
}
