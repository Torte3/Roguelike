#nullable enable
namespace Domain.Model.WorldEvents
{
    public abstract record AlwaysVisibleEvent() : WorldEvent(true);
}
