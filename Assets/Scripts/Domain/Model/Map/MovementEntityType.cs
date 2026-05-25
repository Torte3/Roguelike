using System;

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
    }
}