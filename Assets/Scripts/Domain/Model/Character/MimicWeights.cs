#nullable enable
using System;
using Sirenix.OdinInspector;
using Utilities;


#if UNITY_EDITOR
#endif

namespace Domain.Model.Character
{
    [Serializable]
    public class MimicWeights
    {
        [MinValue(0)] public int ItemRevealOnPickUp = 1;
        [MinValue(0)] public int ItemRevealOnUse = 1;
        [MinValue(0)] public int Money = 1;
        [MinValue(0)] public int Stairs = 1;
        public int GetRandomIndex()
        {
            return RandUtils.WeightedIndex(ItemRevealOnPickUp, ItemRevealOnUse, Money, Stairs);
        }
    }
}