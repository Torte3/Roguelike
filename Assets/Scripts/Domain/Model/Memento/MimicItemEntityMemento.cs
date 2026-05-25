using System;
using Domain.Model.Character;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MimicItemMemento
    {
        [field: SerializeField] public ItemEntityMemento ItemEntity { get; private set; }
        [field: SerializeField] public ScriptableObjectSerializable<EnemyData> Mimic { get; private set; }
        public MimicItemMemento(ItemEntityMemento item, EnemyData mimic)
        {
            ItemEntity = item;
            Mimic = mimic.ToSerializable();
        }
    }
}