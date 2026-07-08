#nullable enable
using Domain.Model.Item.Description;

namespace Domain.Model.WorldEvents
{
    public record ItemEntityLabel(ItemName Item) : EntityLabel;
}
