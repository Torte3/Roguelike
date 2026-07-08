#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class MoneyEntityLabelText : EntityLabelText<MoneyEntityLabel>
    {
        protected override string Of(MoneyEntityLabel label) => $"{label.Amount}G";
    }
}
