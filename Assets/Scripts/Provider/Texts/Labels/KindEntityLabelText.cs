#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class KindEntityLabelText : EntityLabelText
    {
        private readonly KindEntityLabel _label;

        public KindEntityLabelText(KindEntityLabel label)
        {
            _label = label;
        }

        public override string Text => _label.Kind.Name();
    }
}
