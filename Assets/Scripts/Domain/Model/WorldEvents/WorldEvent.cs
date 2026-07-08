#nullable enable

namespace Domain.Model.WorldEvents
{
    public abstract record WorldEvent(bool IsVisible);

    public abstract record AlwaysVisibleEvent() : WorldEvent(true);
}
