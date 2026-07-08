#nullable enable
namespace Domain.Model.Item.Rarities
{
    public interface IHasRarity
    {
        Rarity Rarity { get; }
    }
}