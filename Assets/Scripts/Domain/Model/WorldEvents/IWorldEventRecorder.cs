#nullable enable
namespace Domain.Model.WorldEvents
{
    public interface IWorldEventRecorder
    {
        public void Record(WorldEvent worldEvent);
    }
}
