#nullable enable
using System;
using Domain.Model.Entity;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DeathCountMemento
    {
        [SerializeField] private CharacterLabelMemento _victim;
        [SerializeField] private DamageSourceMemento _source;
        [field: SerializeField] public int Count { get; private set; }

        public DeathCountMemento(DeathRecord record, int count)
        {
            _victim = new CharacterLabelMemento(record.Victim);
            _source = new DamageSourceMemento(record.Source);
            Count = count;
        }

        public DeathRecord Deserialize()
        {
            return new DeathRecord(_victim.Deserialize(), _source.Deserialize());
        }
    }
}
