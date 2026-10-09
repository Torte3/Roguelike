#nullable enable
using Domain.Model.Effect;

namespace Domain.Model.Character
{
    public record ChargeState(int Turns, EffectArea Area);
}
