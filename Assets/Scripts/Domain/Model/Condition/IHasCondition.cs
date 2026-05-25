using Domain.Model.Character;
using Domain.Model.Entity;

namespace Domain.Model.Condition
{
    public interface IHasCondition : IEntity, IHasStatus, IHasAffiliation, IHasName
    {
    }
}