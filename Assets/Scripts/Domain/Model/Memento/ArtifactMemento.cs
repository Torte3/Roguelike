#nullable enable

using System;
using System.Collections.Generic;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ArtifactMemento : IItemMemento
    {
        [field: SerializeField] public BaseItemMemento BaseItem { get; private set; }

        [field: SerializeField]
        public List<ArtifactPassiveConditionBundle> PassiveConditionSlots { get; private set; }

        [field: SerializeField] public int SlotLimit { get; private set; }

        public ArtifactMemento(
            BaseItemMemento baseItem,
            List<ArtifactPassiveConditionBundle> passiveConditionSlots,
            int slotLimit)
        {
            BaseItem = baseItem;
            PassiveConditionSlots = passiveConditionSlots;
            SlotLimit = slotLimit;
        }

        public ArtifactMemento CopyWith(
            BaseItemMemento? baseItem = null,
            List<ArtifactPassiveConditionBundle>? passiveConditionSlots = null,
            int? slotLimit = null)
        {
            return new ArtifactMemento(
                baseItem ?? BaseItem,
                passiveConditionSlots ?? PassiveConditionSlots,
                slotLimit ?? SlotLimit);
        }
    }
}
