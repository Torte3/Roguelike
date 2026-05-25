using System.Collections.Generic;

namespace Domain.Model
{
    public interface IHasPlayerEvent
    {
        public IReadOnlyList<IPlayerEvent> Events { get; }
    }
}