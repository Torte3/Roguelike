using System;
using Domain.Model.Character;
using Domain.Model.Map;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MimicStairsMemento
    {
        [field: SerializeField] public MovementEntityType Type { get; private set; }
        [field: SerializeField] public EntityMemento Entity { get; private set; }
        [field: SerializeField] public ScriptableObjectSerializable<EnemyData> Mimic { get; private set; }
        public MimicStairsMemento(MovementEntityType type, EntityMemento entity, EnemyData mimic)
        {
            Type = type;
            Entity = entity;
            Mimic = mimic.ToSerializable();
        }
    }
}