#nullable enable
using Domain.Model.Character;
using View.Playback;

namespace Provider.Presentations
{
    internal static class Charges
    {
        public static ChargePreview? Of(ChargeState? charge)
        {
            return charge == null ? null : new ChargePreview(charge.Area.Positions, charge.Area.Color, charge.Turns);
        }
    }
}
