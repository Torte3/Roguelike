using Domain.Model.Effect.Area;

namespace Domain.Model.Effect
{
    public static class EffectTargetDescription
    {
        public static string OnUse(IEffectPosition position, IArea area, bool useOrThrowCombinedTargets)
        {
            if (area.IsSelf)
            {
                if (position.IsAtFeet)
                    return useOrThrowCombinedTargets ? "使用者/命中地点を対象に" : "使用者を対象に";
                return $"{position.Description()}を対象に";
            }

            if (position.IsAtFeet)
                return $"{area.Description()}を対象に";
            return $"{position.Description()}の{area.Description()}を対象に";
        }

        public static string OnThrow(IEffectPosition position, IArea area)
        {
            if (area.IsSelf)
                return "命中地点を対象に";
            if (position.IsAtFeet)
                return $"{area.Description()}を対象に";
            return $"{position.Description()}の{area.Description()}を対象に";
        }
    }
}
