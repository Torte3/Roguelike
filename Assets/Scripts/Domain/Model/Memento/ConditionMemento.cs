using System;
using Domain.Model.Character;
using UnityEngine;
using Utilities.Serialize;

namespace Domain.Model.Memento
{
    [Serializable]
    public class ConditionMemento
    {
        [SerializeReference] private ScriptableObjectSerializable<ConditionTemplate> _condition;
        public ConditionTemplate Condition => _condition.Value;
        [field: SerializeField] public int ElapsedTurns { get; private set; }

        public ConditionMemento(ConditionTemplate condition, int elapsedTurns)
        {
            _condition = condition.ToSerializable();
            ElapsedTurns = elapsedTurns;
        }
    }
}