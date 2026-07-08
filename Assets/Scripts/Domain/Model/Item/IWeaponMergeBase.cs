#nullable enable
using System.Collections.Generic;

namespace Domain.Model.Item
{
    public interface IWeaponMergeBase
    {
        public bool CanTakeFeatures(IReadOnlyList<ItemFeature> features, int upgradeCount);
        public IItem TakeFeatures(IReadOnlyList<ItemFeature> features, int upgradeCount);
    }
}
