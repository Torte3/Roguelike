#nullable enable
using System;
using Domain.Model;

namespace Provider
{
    internal static class UnlockText
    {
        public static string Of(UnlockCondition unlock, string description)
        {
            return $"解放条件\n{Condition(unlock)}\n\n{description}";
        }

        private static string Condition(UnlockCondition unlock)
        {
            if (unlock.IsUnlocked || unlock.Required <= 0)
                return unlock.Description;

            return $"{unlock.Description}\n進捗: {Math.Min(unlock.Current, unlock.Required)}/{unlock.Required}";
        }
    }
}
