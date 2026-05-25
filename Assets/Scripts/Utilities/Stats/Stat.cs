using System;
using R3;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Utilities.Stats
{
    public class Stat : IDisposable, IStat
    {
        private float _baseValue;
        private float _additiveValue;
        private float _additiveMultiplier = 1;
        private float _additiveDivisor = 1;
        private float _multiplicativeMultiplier = 1;
        private Option<float> _minValue;
        private Option<float> _maxValue;

        private ReactiveProperty<float> _value = new();
        public ReadOnlyReactiveProperty<float> Value => _value;
        public float CurrentValue => _value.CurrentValue;

        public Stat(float baseValue, float? minValue = null, float? maxValue = null)
        {
            _baseValue = baseValue;
            _additiveValue = 0f;
            _additiveMultiplier = 1f;
            _additiveDivisor = 1f;
            _multiplicativeMultiplier = 1f;
            _minValue = minValue.ToOption();
            _maxValue = maxValue.ToOption();
            if (minValue != null && maxValue != null && minValue > maxValue)
            {
                throw new ArgumentException("Min value cannot be greater than max value");
            }
            SetValue();
        }

        public Stat(StatData data)
        {
            _baseValue = data.BaseValue;
            _additiveValue = data.AdditiveValue;
            _additiveMultiplier = data.AdditiveMultiplier;
            _additiveDivisor = data.AdditiveDivisor;
            _multiplicativeMultiplier = data.MultiplicativeMultiplier;
            _minValue = data.MinValue;
            _maxValue = data.MaxValue;
            if (_minValue.IsSome(out var minValue) && _maxValue.IsSome(out var maxValue) && minValue > maxValue)
            {
                throw new ArgumentException("Min value cannot be greater than max value");
            }
            SetValue();
        }

        public void Dispose()
        {
            _value.Dispose();
        }

        public StatData GetData()
        {
            return new StatData(
                _baseValue,
                _additiveValue,
                _additiveMultiplier,
                _additiveDivisor,
                _multiplicativeMultiplier,
                _minValue,
                _maxValue);
        }

        public void Add(float value)
        {
            _additiveValue += value;
            SetValue();
        }

        public void AddMultiplier(float multiplier)
        {
            _additiveMultiplier += multiplier;
            SetValue();
        }

        public void AddDivisor(float divisor)
        {
            _additiveDivisor += divisor;
            SetValue();
        }

        public void Multiply(float value)
        {
            _multiplicativeMultiplier *= value;
            SetValue();
        }

        public void Remove(float value)
        {
            Add(-value);
        }

        public void RemoveMultiplier(float value)
        {
            AddMultiplier(-value);
        }

        public void RemoveDivisor(float value)
        {
            AddDivisor(-value);
        }

        public void Divide(float value)
        {
            Multiply(1 / value);
        }

        private void SetValue()
        {
            var value = (_baseValue + _additiveValue) * _additiveMultiplier / _additiveDivisor * _multiplicativeMultiplier;
            if (_minValue.IsSome(out var minValue) && value < minValue)
            {
                value = Mathf.Max(value, minValue);
            }
            if (_maxValue.IsSome(out var maxValue) && value > maxValue)
            {
                value = Mathf.Min(value, maxValue);
            }
            _value.Value = value;
        }
    }
}