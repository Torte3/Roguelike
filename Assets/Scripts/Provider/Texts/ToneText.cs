#nullable enable
using System;
using Domain.Model;

namespace Provider.Texts
{
    internal sealed class ToneText
    {
        private readonly ChoiceMessage? _message;

        public ToneText(ChoiceMessage? message)
        {
            _message = message;
        }

        public string? Text => _message == null ? null : Painted(_message.Text, _message.Tone);

        private static string Painted(string text, TextTone tone)
        {
            return tone switch
            {
                TextTone.Plain => text,
                TextTone.Calm => Tint.Calm.Paint(text),
                TextTone.Caution => Tint.Caution.Paint(text),
                TextTone.Danger => Tint.Danger.Paint(text),
                _ => throw new ArgumentOutOfRangeException(nameof(tone), tone, null),
            };
        }
    }
}
