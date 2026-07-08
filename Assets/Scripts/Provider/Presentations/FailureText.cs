#nullable enable
using System;
using Domain.Model.WorldEvents;

namespace Provider.Presentations
{
    internal static class FailureText
    {
        public static string Of(ItemActionFailed failed)
        {
            var item = Names.Of(failed.ItemName);
            return failed.Failure switch
            {
                ItemActionFailure.CursedCannotThrow => $"{item}は呪われていて投げられない",
                ItemActionFailure.CursedCannotDiscard => $"{item}は呪われていて捨てられない",
                ItemActionFailure.CursedCannotUse => $"{item}は呪われているため使用できない",
                ItemActionFailure.CursedCannotUnequip => $"{item}は呪われていて外せない",
                ItemActionFailure.Illiterate => $"{item}は文字が読めない",
                ItemActionFailure.CannotGive => $"{item}を渡せなかった。",
                _ => throw new ArgumentOutOfRangeException(nameof(failed), failed.Failure, null),
            };
        }

        public static string Of(FacilityItemFailed failed)
        {
            var item = Names.Of(failed.ItemName);
            return failed.Failure switch
            {
                FacilityFailure.CursedCannotPutIn => $"{item}は呪われていて入れられない",
                FacilityFailure.CannotTakeOut => $"{item}は取り出せなかった",
                FacilityFailure.CannotPickUp => $"{item}を拾えなかった",
                _ => throw new ArgumentOutOfRangeException(nameof(failed), failed.Failure, null),
            };
        }
    }
}
