using System;
using Utilities.Serialize.Option;

namespace Utilities.Stats
{
    [Serializable]
    public class StatData
    {
        public float BaseValue;
        public float AdditiveValue;
        public float AdditiveMultiplier;
        public float AdditiveDivisor;
        public float MultiplicativeMultiplier;
        public Option<float> MinValue;
        public Option<float> MaxValue;

        public StatData(
            float baseValue, 
            float additiveValue,
            float additiveMultiplier,
            float additiveDivisor,
            float multiplicativeMultiplier,
            Option<float> minValue,
            Option<float> maxValue)
        {
            BaseValue = baseValue;
            AdditiveValue = additiveValue;
            AdditiveMultiplier = additiveMultiplier;
            AdditiveDivisor = additiveDivisor;
            MultiplicativeMultiplier = multiplicativeMultiplier;
            MinValue = minValue;
            MaxValue = maxValue;
        }

        public StatData(float baseValue, float? minValue = null, float? maxValue = null)
            : this(baseValue, 0, 1, 1, 1, minValue.ToOption(), maxValue.ToOption()) {}
    }
}