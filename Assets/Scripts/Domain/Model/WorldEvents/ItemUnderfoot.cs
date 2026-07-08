#nullable enable
namespace Domain.Model.WorldEvents
{
    public sealed record ItemUnderfoot(ItemLook Item) : Underfoot;
}
