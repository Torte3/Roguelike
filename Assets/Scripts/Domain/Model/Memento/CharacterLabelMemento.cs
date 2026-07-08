#nullable enable
using System;
using Domain.Model.Character;
using Domain.Model.Character.Affiliation;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterLabelMemento
    {
        [SerializeField] private string _name;
        [SerializeField] private AffiliationType _affiliation;

        public CharacterLabelMemento(CharacterLabel label)
        {
            _name = label.Name;
            _affiliation = label.Affiliation;
        }

        public CharacterLabel Deserialize()
        {
            return new CharacterLabel(_name, _affiliation);
        }
    }
}
