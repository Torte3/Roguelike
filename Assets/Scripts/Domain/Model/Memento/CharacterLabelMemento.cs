#nullable enable
using System;
using Domain.Model.Character;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterLabelMemento
    {
        [SerializeField] private string _name;
        [SerializeField] private bool _isPlayer;
        [SerializeField] private AffiliationType _affiliation;

        public CharacterLabelMemento(CharacterLabel label)
        {
            _name = label.Name;
            _isPlayer = label.IsPlayer;
            _affiliation = label.Affiliation;
        }

        public CharacterLabel Deserialize()
        {
            return new CharacterLabel(_name, _isPlayer, _affiliation);
        }
    }
}
