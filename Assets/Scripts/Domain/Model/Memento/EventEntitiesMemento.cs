using System;
using System.Collections.Generic;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class EventEntitiesMemento
    {
        [field: SerializeField] public List<MimicItemMemento> MimicItems { get; private set; }
        [field: SerializeField] public List<MimicMoneyMemento> MimicMoney { get; private set; }
        [field: SerializeField] public List<MimicStairsMemento> MimicStairs { get; private set; }
        [field: SerializeField] public List<StairsMemento> Stairs { get; private set; }
        [field: SerializeField] public List<ChestMemento> Chests { get; private set; }
        [field: SerializeField] public List<TrapMemento> Traps { get; private set; }
        [field: SerializeField] public List<StatueMemento> Statues { get; private set; }
        [field: SerializeField] public List<MoneyMemento> Money { get; private set; }
        [field: SerializeField] public Option<BonfireMemento> Bonfire { get; private set; }
        [field: SerializeField] public Option<MagicPotMemento> MagicPot { get; private set; }
        [field: SerializeField] public Option<WorkbenchMemento> Workbench { get; private set; }
        [field: SerializeField] public Option<EntityMemento> Teleporter { get; private set; }

        public EventEntitiesMemento(
            List<MimicItemMemento> mimicItems,
            List<MimicMoneyMemento> mimicMoney,
            List<MimicStairsMemento> mimicStairs,
            List<StairsMemento> stairs,
            List<ChestMemento> chests,
            List<TrapMemento> traps,
            List<StatueMemento> statues,
            List<MoneyMemento> money,
            Option<BonfireMemento> bonfire,
            Option<MagicPotMemento> magicPot,
            Option<WorkbenchMemento> workbench,
            Option<EntityMemento> teleporter)
        {
            MimicItems = mimicItems;
            MimicMoney = mimicMoney;
            MimicStairs = mimicStairs;
            Stairs = stairs;
            Chests = chests;
            Traps = traps;
            Statues = statues;
            Money = money;
            Bonfire = bonfire;
            MagicPot = magicPot;
            Workbench = workbench;
            Teleporter = teleporter;
        }
    }
}