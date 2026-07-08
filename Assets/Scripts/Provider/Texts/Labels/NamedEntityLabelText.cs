#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class NamedEntityLabelText : EntityLabelText
    {
        private readonly NamedEntityLabel _label;

        public NamedEntityLabelText(NamedEntityLabel label)
        {
            _label = label;
        }

        public override string Text => _label.Name;
    }
}
