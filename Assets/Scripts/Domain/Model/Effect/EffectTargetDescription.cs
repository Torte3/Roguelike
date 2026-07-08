using Domain.Model.Effect.Area;
using Domain.Model.Effect.Position;

namespace Domain.Model.Effect
{
    public static class EffectTargetDescription
    {
        public static string OnUse(IEffectPosition position, IArea area, bool useOrThrowCombinedTargets)
        {
            if (area is SelfArea)
            {
                if (position is AtFeet)
                    return useOrThrowCombinedTargets ? "使用者/命中地点を対象に" : "使用者を対象に";
                return $"{position.Description()}を対象に";
            }

            if (position is AtFeet)
                return $"{area.Description()}を対象に";
            return $"{position.Description()}の{area.Description()}を対象に";
        }

        public static string OnThrow(IEffectPosition position, IArea area)
        {
            if (area is SelfArea)
                return "命中地点を対象に";
            if (position is AtFeet)
                return $"{area.Description()}を対象に";
            return $"{position.Description()}の{area.Description()}を対象に";
        }
    }
}
