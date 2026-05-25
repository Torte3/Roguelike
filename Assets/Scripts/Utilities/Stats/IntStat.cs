using System;
using R3;
using UnityEngine;

namespace Utilities.Stats
{
    public class IntStat : IDisposable, IStat
    {
        private readonly Stat _stat;
        public readonly ReadOnlyReactiveProperty<int> IntValue;
        public int CurrentIntValue => Mathf.RoundToInt(_stat.CurrentValue);

        public ReadOnlyReactiveProperty<float> Value => _stat.Value;
        public float CurrentValue => _stat.CurrentValue;

        public IntStat(float baseValue)
        {
            _stat = new Stat(baseValue);
            IntValue = _stat.Value.Select(x => Mathf.RoundToInt(x)).ToReadOnlyReactiveProperty();
        }

        public IntStat(StatData data)
        {
            _stat = new Stat(data);
            IntValue = _stat.Value.Select(x => Mathf.RoundToInt(x)).ToReadOnlyReactiveProperty();
        }

        public void Dispose()
        {
            _stat.Dispose();
        }

        public StatData GetData()
        {
            return _stat.GetData();
        }

        public void Add(float value)
        {
            _stat.Add(value);
        }

        public void AddMultiplier(float multiplier)
        {
            _stat.AddMultiplier(multiplier);
        }

        public void AddDivisor(float divisor)
        {
            _stat.AddDivisor(divisor);
        }

        public void Multiply(float multiplier)
        {
            _stat.Multiply(multiplier);
        }

        public void Remove(float value)
        {
            _stat.Add(-value);
        }

        public void RemoveMultiplier(float value)
        {
            _stat.AddMultiplier(-value);
        }

        public void RemoveDivisor(float value)
        {
            _stat.AddDivisor(-value);
        }

        public void Divide(float value)
        {
            _stat.Multiply(1 / value);
        }
    }
}