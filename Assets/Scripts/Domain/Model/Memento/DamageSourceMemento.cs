#nullable enable
using System;
using Domain.Model.Entity;
using UnityEngine;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class DamageSourceMemento
    {
        [SerializeField] private DamageCause _cause;
        [SerializeField] private CollisionRole _collision;
        [SerializeField] private Option<CharacterLabelMemento> _opponentLabel;
        [SerializeField] private bool _opponentIsVisible;

        public DamageSourceMemento(DamageSource source)
        {
            _cause = source.Cause;
            _collision = source.Collision;
            _opponentLabel = source.Opponent.ToOption().Map(opponent => new CharacterLabelMemento(opponent.Label));
            _opponentIsVisible = source.Opponent?.IsVisible ?? false;
        }

        public DamageSource Deserialize()
        {
            var opponent = _opponentLabel.Map(label => new Opponent(label.Deserialize(), _opponentIsVisible)).UnwrapOrNull();
            return new DamageSource(_cause, opponent, _collision);
        }
    }
}
