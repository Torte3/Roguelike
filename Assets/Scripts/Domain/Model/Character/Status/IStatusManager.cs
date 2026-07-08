#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using ObservableCollections;
using Utilities.Stats;

namespace Domain.Model.Character.Status
{
    public interface IStatusManager : IReadOnlyStatus
    {
        public IObservableCollection<ICondition> Conditions { get; }
        public UniTask UpdateTurn(bool enemyVisible);
        public IStat GetStat(StatType type);
        public IStat GetAttackMultiplierStat();
        public IStat GetElementAttackMultiplierStat(Element element);
        public IStat GetElementDamageRateMultiplierStat(Element element);
        public IStat GetConditionResistanceStat(ConditionTemplate condition);
        public IFlagStat GetFlagStat(FlagStatType type);
        public void AddWaitTime(float value);
        public void ResetWaitTime();
    }
}