#nullable enable
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.WorldEvents;
using Provider.Texts;
using Provider.Texts.Labels;

namespace Provider.Presentations
{
    internal static class Names
    {
        public static string Of(CharacterLabel label) => CharacterNameText.Of(label, true);

        public static string Of(ItemName name) => ItemNameText.Of(name);

        public static string Of(EntityLabel label) => EntityLabelTexts.Of(label);
    }
}
