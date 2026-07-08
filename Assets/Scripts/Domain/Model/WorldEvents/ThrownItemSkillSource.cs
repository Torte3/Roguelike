#nullable enable
using Domain.Model.Item.Description;

namespace Domain.Model.WorldEvents
{
    public record ThrownItemSkillSource(ItemName ItemName) : SkillSource;
}
