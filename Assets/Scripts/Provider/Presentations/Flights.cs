#nullable enable
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Ops;

namespace Provider.Presentations
{
    internal static class Flights
    {
        public static ViewOp Fly(Flight flight) => new FlyProjectile(flight.Icon, flight.From, flight.To);
    }
}
