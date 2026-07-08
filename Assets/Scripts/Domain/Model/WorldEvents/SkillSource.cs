#nullable enable
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;

namespace Domain.Model.WorldEvents
{
    public abstract record SkillSource;

    public record CharacterSkillSource(CharacterLabel Label, string Log) : SkillSource;

    public record ItemSkillSource(CharacterLabel Label, ItemName ItemName, string ItemBaseName, ItemCategory Category,
        ItemUseKind Kind) : SkillSource;

    public record DeviceSkillSource(string DeviceName, DeviceKind Kind) : SkillSource;
}
