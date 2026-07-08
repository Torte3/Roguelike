namespace Domain.Model.Character.Behavior
{
    public enum BehaviorState
    {
        None,
        DiscoveringEnemy,
        DiscoveringLeader,
        ApproachingToObserve,
        ReturningHome,
        MovingToLastKnownEnemyPosition,
        MovingToLastKnownLeaderPosition,
        Wandering
    }
}