#nullable enable
using System;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class BehaviorMemento
    {
        [field: SerializeField] public BehaviorData Behavior { get; private set; }
        [field: SerializeField] public Option<Location> HomeLocation { get; private set; }
        [field: SerializeField] public Option<BehaviorState> PreviousState { get; private set; }
        [field: SerializeField] public Option<Location> PreviousTargetLocation { get; private set; }

        public BehaviorMemento(
            BehaviorData behavior,
            Option<Location> homeLocation,
            Option<BehaviorState> previousState,
            Option<Location> previousTargetLocation
        )
        {
            Behavior = behavior;
            HomeLocation = homeLocation;
            PreviousState = previousState;
            PreviousTargetLocation = previousTargetLocation;
        }
    }
}