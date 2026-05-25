namespace Domain.Model
{
    public interface IHasEntityEvent
    {
        IEntityEvent Event { get; }
    }
}
