using R3;

namespace Utilities.Stats
{
    public interface IStat
    {
        public ReadOnlyReactiveProperty<float> Value { get; }
        public float CurrentValue { get; }
        public void Add(float value);
        public void AddMultiplier(float multiplier);
        public void AddDivisor(float divisor);
        public void Multiply(float multiplier);
        public void Remove(float value);
        public void RemoveMultiplier(float value);
        public void RemoveDivisor(float value);
        public void Divide(float value);
    }
}