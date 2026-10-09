#nullable enable
using R3;
using Utilities.Stats;

namespace Domain.Service.Characters
{
    internal sealed class RecordingStat : IStat
    {
        private readonly IStat _stat;
        private readonly System.Action _record;

        public RecordingStat(IStat stat, System.Action record)
        {
            _stat = stat;
            _record = record;
        }

        public ReadOnlyReactiveProperty<float> Value => _stat.Value;
        public float CurrentValue => _stat.CurrentValue;

        public void Add(float value) => Change(() => _stat.Add(value));
        public void AddMultiplier(float multiplier) => Change(() => _stat.AddMultiplier(multiplier));
        public void AddDivisor(float divisor) => Change(() => _stat.AddDivisor(divisor));
        public void Multiply(float multiplier) => Change(() => _stat.Multiply(multiplier));
        public void Remove(float value) => Change(() => _stat.Remove(value));
        public void RemoveMultiplier(float value) => Change(() => _stat.RemoveMultiplier(value));
        public void RemoveDivisor(float value) => Change(() => _stat.RemoveDivisor(value));
        public void Divide(float value) => Change(() => _stat.Divide(value));

        private void Change(System.Action change)
        {
            change();
            _record();
        }
    }
}
