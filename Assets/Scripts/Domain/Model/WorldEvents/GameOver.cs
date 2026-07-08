#nullable enable
using Domain.Model.Entity;

namespace Domain.Model.WorldEvents
{
    public record GameOver(DeathRecord Death, int MaxMapLevel, float Score) : AlwaysVisibleEvent;
}
