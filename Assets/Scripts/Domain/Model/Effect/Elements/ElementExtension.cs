using System;

namespace Domain.Model.Effect.Elements
{
    public static class ElementExtension
    {
        public static string Name(this Element element)
        {
            return element switch
            {
                Element.Physical => "物理",
                Element.Fire => "火",
                Element.Ice => "氷",
                Element.Thunder => "雷",
                Element.Light => "光",
                Element.Dark => "闇",
                _ => throw new ArgumentOutOfRangeException(nameof(element), element, null)
            };
        }
    }
}