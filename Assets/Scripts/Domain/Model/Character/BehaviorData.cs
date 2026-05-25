#nullable enable
using System;
using Sirenix.OdinInspector;

namespace Domain.Model.Character
{
    [Serializable]
    public class BehaviorData
    {
        public bool wanderAround = true;
        public MoveTypeWhenDiscoveringTarget Default = MoveTypeWhenDiscoveringTarget.Chase;
        public bool PrioritizeMovement;
        public bool UseTopBound;

        [ShowIf(nameof(UseTopBound))]
        public MoveTypeWhenDiscoveringTarget greaterThanTopBound = MoveTypeWhenDiscoveringTarget.Chase;

        [ShowIf(nameof(UseTopBound))] [MinValue(0)] public float distanceTopBound = 6f;
        [ShowIf(nameof(UseTopBound))] public bool PrioritizeMovementWhenDistanceGreaterThanTopBound;
        public bool UseBottomBound;

        [ShowIf(nameof(UseBottomBound))] [MinValue(0)]
        public float distanceBottomBound = 3f;

        [ShowIf(nameof(UseBottomBound))]
        public MoveTypeWhenDiscoveringTarget lessThanBottomBound = MoveTypeWhenDiscoveringTarget.Chase;

        [ShowIf(nameof(UseBottomBound))] public bool PrioritizeMovementWhenDistanceLessThanBottomBound;

        public bool PrioritizeEnemiesOverLeaders;
        public bool ChaseLeader = true;
    }
}