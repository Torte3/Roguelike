using System;

namespace Domain.Model.Character.Status
{
    public static class MoveSpeedExtensions
    {
        public static float ToWaitTime(this MoveSpeed moveSpeed)
        {
            return moveSpeed switch
            {
                MoveSpeed.Quarter => 4,
                MoveSpeed.Half => 2,
                MoveSpeed.Normal => 1,
                MoveSpeed.Double => 0.5f,
                MoveSpeed.Quadruple => 0.25f,
                _ => throw new ArgumentException("Invalid MoveSpeed")
            };
        }
        public static string GetName(this MoveSpeed moveSpeed)
        {
            return moveSpeed switch
            {
                MoveSpeed.Quarter => "超鈍足",
                MoveSpeed.Half => "鈍足",
                MoveSpeed.Normal => "通常",
                MoveSpeed.Double => "倍速",
                MoveSpeed.Quadruple => "4倍速",
            };
        }
    }
}