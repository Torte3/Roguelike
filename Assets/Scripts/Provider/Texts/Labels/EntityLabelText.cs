#nullable enable
using System;
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal abstract class EntityLabelText
    {
        public static EntityLabelText Of(EntityLabel label)
        {
            return label switch
            {
                CharacterEntityLabel character => new CharacterEntityLabelText(character),
                ItemEntityLabel item => new ItemEntityLabelText(item),
                MoneyEntityLabel money => new MoneyEntityLabelText(money),
                NamedEntityLabel named => new NamedEntityLabelText(named),
                KindEntityLabel kind => new KindEntityLabelText(kind),
                _ => throw new ArgumentOutOfRangeException(nameof(label), label, null),
            };
        }

        public abstract string Text { get; }
    }
}
