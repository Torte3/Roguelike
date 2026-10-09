#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal static class EntityLabelTexts
    {
        private static readonly IReadOnlyDictionary<Type, IEntityLabelText> s_texts = new IEntityLabelText[]
        {
            new CharacterEntityLabelText(),
            new ItemEntityLabelText(),
            new MoneyEntityLabelText(),
            new NamedEntityLabelText(),
            new KindEntityLabelText(),
        }.ToDictionary(text => text.LabelType);

        public static string Of(EntityLabel label) => s_texts[label.GetType()].Of(label);
    }
}
