#nullable enable

using System;
using System.Collections.Generic;
using Domain.Model.Item;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EquipmentItemMemento : IItemMemento
    {
        [field: SerializeField] public BaseItemMemento BaseItem { get; private set; }

        [field: SerializeField]
        public List<ArtifactPassiveConditionBundle> PassiveConditionSlots { get; private set; }

        [field: SerializeField] public int SlotLimit { get; private set; }

        [field: SerializeField] public bool IsEquipped { get; private set; }

        public EquipmentItemMemento(
            BaseItemMemento baseItem,
            List<ArtifactPassiveConditionBundle> passiveConditionSlots,
            int slotLimit,
            bool isEquipped = false)
        {
            BaseItem = baseItem;
            PassiveConditionSlots = passiveConditionSlots;
            SlotLimit = slotLimit;
            IsEquipped = isEquipped;
        }

        public EquipmentItemMemento CopyWith(
            BaseItemMemento? baseItem = null,
            List<ArtifactPassiveConditionBundle>? passiveConditionSlots = null,
            int? slotLimit = null,
            bool? isEquipped = null)
        {
            return new EquipmentItemMemento(
                baseItem ?? BaseItem,
                passiveConditionSlots ?? PassiveConditionSlots,
                slotLimit ?? SlotLimit,
                isEquipped ?? IsEquipped);
        }
    }
}
