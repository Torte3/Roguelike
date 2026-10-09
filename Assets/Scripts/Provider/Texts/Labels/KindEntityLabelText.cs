#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class KindEntityLabelText : EntityLabelText<KindEntityLabel>
    {
        protected override string Of(KindEntityLabel label) => label.Kind.Name();
    }
}
