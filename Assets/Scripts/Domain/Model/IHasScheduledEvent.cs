#nullable enable
namespace Domain.Model
{
    public interface IHasScheduledEvent
    {
        public IScheduledEvent Event { get; }
    }
}