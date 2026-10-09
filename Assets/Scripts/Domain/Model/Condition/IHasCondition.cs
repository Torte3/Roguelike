using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Entity;

namespace Domain.Model.Condition
{
    public interface IHasCondition : IEntity, IHasStatus, IHasAffiliation, IHasLabel
    {
        public new IStatusManager Status { get; }
    }
}