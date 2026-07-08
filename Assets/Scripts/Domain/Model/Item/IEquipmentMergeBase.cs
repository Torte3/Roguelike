#nullable enable
using System.Collections.Generic;
using Domain.Model.Item.Data;

namespace Domain.Model.Item
{
    public interface IEquipmentMergeBase
    {
        public bool CanTakePassiveSlots(IReadOnlyList<ArtifactPassiveConditionBundle> slots);
        public IItem TakePassiveSlots(IReadOnlyList<ArtifactPassiveConditionBundle> slots);
    }
}
