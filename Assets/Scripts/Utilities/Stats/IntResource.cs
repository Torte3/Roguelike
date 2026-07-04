using System;
using R3;
using UnityEngine;

namespace Utilities.Stats
{
    public class IntResource : IDisposable
    {
        public readonly IntStat _max;
        private readonly ReactiveProperty<float> _value;

        public IntResource(int maxValue) : this(new IntStat(maxValue), maxValue)
        {
        }

        public IntResource(int maxValue, int value) : this(new IntStat(maxValue), value)
        {
        }

        public IntResource(ResourceData data) : this(new IntStat(data.Max), data.Value)
        {
        }

        private IntResource(IntStat max, float value)
        {
            _max = max;
            _value = new ReactiveProperty<float>(value);
            Value = _value.Select(v => Mathf.FloorToInt(v)).ToReadOnlyReactiveProperty();
            Max.IntValue.Subscribe(_ => clampCurrentValue());
        }

        public IntStat Max => _max;

        public ReadOnlyReactiveProperty<int> Value { get; }

        public void Dispose()
        {
            _value.Dispose();
        }

        public ResourceData GetData()
        {
            return new ResourceData(_max.GetData(), _value.Value);
        }

        private void clampCurrentValue()
        {
            _value.Value = Mathf.Clamp(Value.CurrentValue, 0, Max.CurrentIntValue);
        }

        public int Lose(float value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Value cannot be negative");
            }

            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue - value, 0, Max.CurrentIntValue);
            return oldValue - Value.CurrentValue;
        }

        public int Gain(float value)
        {
            if (value < 0)
            {
                throw new ArgumentException("Value cannot be negative");
            }

            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(_value.CurrentValue + value, 0, Max.CurrentIntValue);
            return Value.CurrentValue - oldValue;
        }

        public void Set(float value)
        {
            _value.Value = Mathf.Clamp(value, 0, Max.CurrentIntValue);
        }
    }
}