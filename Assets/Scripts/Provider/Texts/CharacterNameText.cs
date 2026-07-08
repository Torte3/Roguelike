#nullable enable
using Domain.Model.Character;
using Domain.Model.Character.Affiliation;

namespace Provider.Texts
{
    internal sealed class CharacterNameText
    {
        public const string Unknown = "何者か";

        private readonly CharacterLabel _label;

        public CharacterNameText(CharacterLabel label)
        {
            _label = label;
        }

        public string Text => TintOf(_label.Affiliation).Paint(_label.Name);

        private static Tint TintOf(AffiliationType affiliation)
        {
            return affiliation switch
            {
                AffiliationType.Ally => Tint.Ally,
                AffiliationType.Enemy => Tint.Enemy,
                _ => Tint.Neutral,
            };
        }
    }
}
