using System;
using R3;
using UnityEngine;

namespace Utilities.Stats
{
    public class Resource : IDisposable
    {
        private readonly Stat _max;
        private readonly ReactiveProperty<float> _value;

        public Resource(float maxValue)
        {
            _max = new Stat(maxValue);
            _value = new ReactiveProperty<float>(maxValue);
            Max.Value.Subscribe(_ => clampCurrentValue());
        }

        public Resource(int maxValue, int value)
        {
            _max = new Stat(maxValue);
            _value = new ReactiveProperty<float>(value);
            Max.Value.Subscribe(_ => clampCurrentValue());
        }

        public Resource(ResourceData data)
        {
            _max = new Stat(data.Max);
            _value = new ReactiveProperty<float>(data.Value);
            Max.Value.Subscribe(_ => clampCurrentValue());
        }

        public Stat Max => _max;
        public ReadOnlyReactiveProperty<float> Value => _value;

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
            _value.Value = Mathf.Clamp(Value.CurrentValue, 0, Max.CurrentValue);
        }

        public void Set(float value)
        {
            _value.Value = Mathf.Clamp(value, 0, Max.CurrentValue);
        }

        public float Lose(float value)
        {
            if (value < 0)
            {
                return -Gain(-value);
            }

            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue - value, 0, Max.CurrentValue);
            return oldValue - _value.Value;
        }

        public float Gain(float value)
        {
            if (value < 0)
            {
                return -Lose(-value);
            }

            var oldValue = Value.CurrentValue;
            _value.Value = Mathf.Clamp(Value.CurrentValue + value, 0, Max.CurrentValue);
            return _value.Value - oldValue;
        }

        public bool IsFull()
        {
            return Value.CurrentValue >= Max.CurrentValue;
        }
    }
}