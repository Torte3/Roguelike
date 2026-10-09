#nullable enable
using System;
using Domain.Model.WorldEvents;

namespace Provider.Texts.Labels
{
    internal interface IEntityLabelText
    {
        public Type LabelType { get; }
        public string Of(EntityLabel label);
    }
}
