#nullable enable
using Domain.Model.Character;
using Domain.Model.Item;

namespace Domain.Model.WorldEvents
{
    public abstract record EntityLabel;

    public record CharacterEntityLabel(CharacterLabel Character) : EntityLabel;

    public record ItemEntityLabel(ItemName Item) : EntityLabel;

    public record MoneyEntityLabel(int Amount) : EntityLabel;

    public record NamedEntityLabel(string Name) : EntityLabel;

    public record KindEntityLabel(FixtureKind Kind) : EntityLabel;
}
