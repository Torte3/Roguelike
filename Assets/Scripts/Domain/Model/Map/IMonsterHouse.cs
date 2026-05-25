#nullable enable
using R3;

namespace Domain.Model.Map
{
    public interface IMonsterHouse
    {
        public ReadOnlyReactiveProperty<bool> HasEverEntered { get; }
    }
}
