#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class CharacterEntityLabelText : EntityLabelText<CharacterEntityLabel>
    {
        protected override string Of(CharacterEntityLabel label) => CharacterNameText.Of(label.Character, true);
    }
}
