using System;
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Entity;
using UnityEngine;
using Utilities;
using Utilities.Serialize;
using Utilities.Stats;

namespace Domain.Model.Memento
{
    [Serializable]
    public class AffiliationMemento
    {
        [field: SerializeField] public CharacterGroup Group { get; private set; }
        [SerializeField] private SerializableDictionary<string, float> _affiliations;

        public Dictionary<Id<IEntity>, float> Affiliations =>
            _affiliations.ToDictionary(x => new Id<IEntity>(x.Key), x => x.Value);

        [SerializeField] private List<string> _forcedAffiliationTargets;
        [SerializeField] private List<AffiliationType> _forcedAffiliationTypes;
        [SerializeField] private List<int> _forcedAffiliationFlags;

        public Dictionary<(Id<IEntity>, AffiliationType), FlagStat> ForcedAffiliationFlags => _forcedAffiliationFlags
            .Select((x, i) => (new Id<IEntity>(_forcedAffiliationTargets[i]), _forcedAffiliationTypes[i],
                new FlagStat(x))).ToDictionary(x => (x.Item1, x.Item2), x => x.Item3);

        public AffiliationMemento(
            CharacterGroup group,
            Dictionary<Id<IEntity>, float> affiliations,
            Dictionary<(Id<IEntity>, AffiliationType), FlagStat> forcedAffiliationFlags)
        {
            Group = group;
            _affiliations = affiliations.ToSerializableDictionary(x => x.Key.ToString(), x => x.Value);
            _forcedAffiliationTargets = forcedAffiliationFlags.Select(x => x.Key.Item1.ToString()).ToList();
            _forcedAffiliationTypes = forcedAffiliationFlags.Select(x => x.Key.Item2).ToList();
            _forcedAffiliationFlags = forcedAffiliationFlags.Select(x => x.Value.CurrentFlags).ToList();
        }
    }
}