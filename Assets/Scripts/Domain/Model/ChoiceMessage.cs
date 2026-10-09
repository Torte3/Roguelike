#nullable enable

namespace Domain.Model
{
    public record ChoiceMessage(string Text, TextTone Tone = TextTone.Plain);
}
