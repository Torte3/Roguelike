#nullable enable
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Item.Description;

namespace Domain.Model.WorldEvents
{
    public record ItemSkillSource(CharacterLabel Label, ItemName ItemName, string ItemBaseName, ItemCategory Category,
        EquipState EquipState) : SkillSource;
}
