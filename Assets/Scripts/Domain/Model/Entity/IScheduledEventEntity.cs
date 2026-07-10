namespace Domain.Model.Entity
{
    public interface IScheduledEventEntity : IEntity
    {
        public IScheduledEvent Event { get; }
    }
}