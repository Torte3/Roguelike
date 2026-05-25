#nullable enable
using Domain.Model.Memento;
using R3;

namespace Domain.Model.Character
{
    public interface IPlayer : ISerializable<PlayerMemento>
    {
        public ICharacter Character { get; }
        public ReadOnlyReactiveProperty<int> Money { get; }
        public int StealCount { get; }
        public void RecordSteal();
        public void AddMoney(int value);
        public void ReduceMoney(int value);
    }
}