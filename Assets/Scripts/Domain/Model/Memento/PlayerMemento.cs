#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class PlayerMemento
    {
        [field: SerializeField] public CharacterMemento Character { get; private set; }
        [field: SerializeField] public int Money { get; private set; }
        [field: SerializeField] public int StealCount { get; private set; }
        public PlayerMemento(CharacterMemento character, int money, int stealCount = 0)
        {
            Character = character;
            Money = money;
            StealCount = stealCount;
        }
        public PlayerMemento CopyWith(CharacterMemento? character = null, int? money = null, int? stealCount = null)
        {
            return new PlayerMemento(character ?? Character, money ?? Money, stealCount ?? StealCount);
        }
    }
}