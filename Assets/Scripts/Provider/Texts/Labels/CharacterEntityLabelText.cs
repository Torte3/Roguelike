#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class CharacterEntityLabelText : EntityLabelText
    {
        private readonly CharacterEntityLabel _label;

        public CharacterEntityLabelText(CharacterEntityLabel label)
        {
            _label = label;
        }

        public override string Text => new CharacterNameText(_label.Character).Text;
    }
}
