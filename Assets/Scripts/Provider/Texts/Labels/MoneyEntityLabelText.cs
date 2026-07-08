#nullable enable
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal sealed class MoneyEntityLabelText : EntityLabelText
    {
        private readonly MoneyEntityLabel _label;

        public MoneyEntityLabelText(MoneyEntityLabel label)
        {
            _label = label;
        }

        public override string Text => $"{_label.Amount}G";
    }
}
