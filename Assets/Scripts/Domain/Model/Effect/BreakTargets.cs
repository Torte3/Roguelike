#nullable enable
using System;

namespace Domain.Model.Effect
{
    [Flags]
    public enum BreakTargets
    {
        None = 0,
        Character = 1 << 0,
        Item = 1 << 1,
        Money = 1 << 2,
        Trap = 1 << 3,
        Chest = 1 << 4,
        Statue = 1 << 5,
    }
}
