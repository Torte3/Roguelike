using System;
using Domain.Model.WorldEvents;

namespace Domain.Model.Map
{
    public enum MovementEntityType
    {
        DownStairs,
        UpStairs,
        MagicCircle
    }

    public static class MovementEntityTypeExtensions
    {
        public static MovementEntityType Reverse(this MovementEntityType type) =>
            type switch
            {
                MovementEntityType.DownStairs => MovementEntityType.UpStairs,
                MovementEntityType.UpStairs => MovementEntityType.DownStairs,
                MovementEntityType.MagicCircle => MovementEntityType.MagicCircle,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };

        public static FixtureKind ToFixtureKind(this MovementEntityType type) =>
            type switch
            {
                MovementEntityType.UpStairs => FixtureKind.UpStairs,
                MovementEntityType.DownStairs => FixtureKind.DownStairs,
                MovementEntityType.MagicCircle => FixtureKind.MagicCircle,
                _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
            };
    }
}