namespace Domain.Model.Effect
{
    public static class EffectTargetDescription
    {
        public const string AtFeetPositionInfo = "発動場所";
        public const string SelfAreaInfo = "その場";

        public static string OnUse(string positionInfo, string areaInfo, bool useOrThrowCombinedTargets)
        {
            if (areaInfo == SelfAreaInfo)
            {
                if (positionInfo == AtFeetPositionInfo)
                    return useOrThrowCombinedTargets ? "使用者/命中地点を対象に" : "使用者を対象に";
                return $"{positionInfo}を対象に";
            }

            if (positionInfo == AtFeetPositionInfo)
                return $"{areaInfo}を対象に";
            return $"{positionInfo}の{areaInfo}を対象に";
        }

        public static string OnThrow(string positionInfo, string areaInfo)
        {
            if (areaInfo == SelfAreaInfo)
                return "命中地点を対象に";
            if (positionInfo == AtFeetPositionInfo)
                return $"{areaInfo}を対象に";
            return $"{positionInfo}の{areaInfo}を対象に";
        }
    }
}
