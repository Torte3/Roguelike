#nullable enable
using System;
using Domain.Model;

namespace Provider.Texts
{
    internal static class ToneText
    {
        public static string? Of(ChoiceMessage? message)
        {
            return message == null ? null : Of(message.Text, message.Tone);
        }

        public static string Of(string text, TextTone tone)
        {
            return tone switch
            {
                TextTone.Plain => text,
                TextTone.Calm => text.Paint(Tint.Calm),
                TextTone.Caution => text.Paint(Tint.Caution),
                TextTone.Danger => text.Paint(Tint.Danger),
                _ => throw new ArgumentOutOfRangeException(nameof(tone), tone, null),
            };
        }
    }
}
