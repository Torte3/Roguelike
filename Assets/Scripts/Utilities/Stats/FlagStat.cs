using R3;

namespace Utilities.Stats
{
    public class FlagStat : IFlagStat
    {
        private readonly ReactiveProperty<int> _flags;
        private readonly ReactiveProperty<bool> _value;
        public bool CurrentValue => _value.Value;
        public int CurrentFlags => _flags.Value;

        public FlagStat(int flags)
        {
            _flags = new ReactiveProperty<int>(flags);
            _value = new ReactiveProperty<bool>(flags > 0);
        }

        public ReadOnlyReactiveProperty<bool> Value => _value;
        public ReadOnlyReactiveProperty<int> Flags => _flags;

        public void Add()
        {
            _flags.Value++;
            _value.Value = _flags.Value > 0;
        }

        public void Remove()
        {
            _flags.Value--;
            _value.Value = _flags.Value > 0;
        }
    }
}