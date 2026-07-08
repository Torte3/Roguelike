#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class NamedEntityLabelText : EntityLabelText<NamedEntityLabel>
    {
        protected override string Of(NamedEntityLabel label) => label.Name;
    }
}
