#nullable enable
using System;
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal abstract class EntityLabelText<TLabel> : IEntityLabelText where TLabel : EntityLabel
    {
        public Type LabelType => typeof(TLabel);

        public string Of(EntityLabel label) => Of((TLabel)label);

        protected abstract string Of(TLabel label);
    }
}
