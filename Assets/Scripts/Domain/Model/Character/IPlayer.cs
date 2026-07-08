#nullable enable
using Domain.Model.Memento;

namespace Domain.Model.Character
{
    public interface IPlayer : ISerializable<PlayerMemento>
    {
        public ICharacter Character { get; }
        public IReadOnlyPlayerCharacter ReadOnlyCharacter { get; }
        public int Money { get; }
        public int StealCount { get; }
        public void RecordSteal();
        public void AddMoney(int value);
        public void ReduceMoney(int value);
    }
}