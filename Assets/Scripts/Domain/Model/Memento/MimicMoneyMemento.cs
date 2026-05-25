using System;
using Domain.Model.Character;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MimicMoneyMemento
    {
        [field: SerializeField] public MoneyMemento Money { get; private set; }
        [field: SerializeField] public ScriptableObjectSerializable<EnemyData> Mimic { get; private set; }
        public MimicMoneyMemento(MoneyMemento money, EnemyData mimic)
        {
            Money = money;
            Mimic = mimic.ToSerializable();
        }
    }
}