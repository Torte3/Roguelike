#nullable enable
using Domain.Model.Effect;
using Domain.Model.WorldEvents;
using R3;
using Utilities.Result;

namespace Domain.Model.Item
{
    public static class ItemChecks
    {
        public static Result<Unit, TFailure> Check<TFailure>(bool passes, TFailure failure)
        {
            return passes ? Result<Unit, TFailure>.Ok(Unit.Default) : Result<Unit, TFailure>.Failed(failure);
        }

        public static Result<Unit, ItemActionFailure> ThrowCheck(this IItem item)
        {
            return Check(item.CanAttemptThrow, ItemActionFailure.CursedCannotThrow);
        }

        public static Result<Unit, ItemActionFailure> DiscardCheck(this IItem item)
        {
            return Check(!item.IsDiscardBlocked, ItemActionFailure.CursedCannotDiscard);
        }

        public static Result<Unit, ItemActionFailure> ReadCheck(this IItem item, IActorOfEffect reader)
        {
            return Check(reader.CanReadItem || !item.RequiresLiteracy, ItemActionFailure.Illiterate);
        }

        public static Result<Unit, FacilityFailure> PutInCheck(this IItem item)
        {
            return Check(!item.IsDiscardBlocked, FacilityFailure.CursedCannotPutIn);
        }

        public static Result<Unit, FacilityFailure> TakeOutCheck(this IStorage storage, IItem item)
        {
            return Check(storage.CanRemove(item), FacilityFailure.CannotTakeOut);
        }

        public static Result<Unit, FacilityFailure> PickUpCheck(this IStorage storage)
        {
            return Check(storage.CanAddToEmpty(), FacilityFailure.CannotPickUp);
        }
    }
}
