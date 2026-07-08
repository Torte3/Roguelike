#nullable enable
using System;
using Domain.Model.Entity;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class OpponentMemento
    {
        [SerializeField] private CharacterLabelMemento _label;
        [SerializeField] private bool _isVisible;

        public OpponentMemento(Opponent opponent)
        {
            _label = new CharacterLabelMemento(opponent.Label);
            _isVisible = opponent.IsVisible;
        }

        public Opponent Deserialize()
        {
            return new Opponent(_label.Deserialize(), _isVisible);
        }
    }
}
