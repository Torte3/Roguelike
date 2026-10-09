#nullable enable
using Domain.Model.Character;

namespace Provider.Texts
{
    internal static class CharacterNameText
    {
        public const string Unknown = "何者か";

        public static string Of(CharacterLabel label, bool isVisible)
        {
            if (!isVisible)
                return Unknown;
            return label.Affiliation switch
            {
                AffiliationType.Ally => label.Name.Paint(Tint.Ally),
                AffiliationType.Enemy => label.Name.Paint(Tint.Enemy),
                _ => label.Name.Paint(Tint.Neutral),
            };
        }
    }
}
