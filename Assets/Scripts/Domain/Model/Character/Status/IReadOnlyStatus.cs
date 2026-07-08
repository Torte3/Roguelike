#nullable enable
using Domain.Model.Effect;
using R3;

namespace Domain.Model.Character.Status
{
    public interface IReadOnlyStatus
    {
        public ReadOnlyReactiveProperty<float> WaitTimeValue { get; }
        public bool IsWaitTimeFull();
        public bool IsFlagStat(FlagStatType type);
        public ReadOnlyReactiveProperty<bool> GetFlagProperty(FlagStatType type);
        public float GetStatValue(StatType type);
        public float GetCombinedElementAttackMultiplier(Element element);
        public float GetElementDamageRateMultiplier(Element element);
        public float GetConditionResistance(ConditionTemplate condition);
    }
}
