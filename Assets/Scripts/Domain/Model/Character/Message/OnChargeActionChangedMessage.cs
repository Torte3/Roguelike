#nullable enable
namespace Domain.Model.Character.Message
{
    public record OnChargeActionUpdatedMessage(int Turn, ChargedActionPreviewEffectData? Data);
}