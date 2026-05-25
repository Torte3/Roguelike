using R3;

namespace Utilities.Stats
{
    public interface IFlagStat
    {
        public ReadOnlyReactiveProperty<bool> Value { get; }
        public bool CurrentValue { get; }
        public ReadOnlyReactiveProperty<int> Flags { get; }
        public int CurrentFlags { get; }
        public void Add();
        public void Remove();
    }
}