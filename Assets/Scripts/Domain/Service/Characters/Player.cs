#nullable enable
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters.Behavior;
using Unity.Logging;

namespace Domain.Service.Characters
{
    internal sealed class Player : IPlayer
    {
        public ICharacter Character { get; }
        public IReadOnlyPlayerCharacter ReadOnlyCharacter { get; }
        public int Money { get; private set; }
        public int StealCount { get; private set; }

        public Player(PlayerMemento data, CharacterControlInputReceiver receiver, IGameManager gameManager, IMap map)
        {
            var character = new Character(data.Character, new PlayerBehavior(receiver, gameManager), map, true);
            Character = character;
            ReadOnlyCharacter = character;
            Money = data.Money;
            StealCount = data.StealCount;
        }

        public void RecordSteal()
        {
            StealCount++;
            if (!Character.Status.IsFlagStat(FlagStatType.StealEmpower))
                return;
            if (StealCount > CommonSenseParameters.StealAttackBonusMaxCount)
                return;

            Character.Status.GetAttackMultiplierStat().Add(CommonSenseParameters.StealAttackBonusPerCount);
        }

        public PlayerMemento Serialize()
        {
            return new PlayerMemento(Character.Serialize(), Money, StealCount);
        }

        public void AddMoney(int value)
        {
            Log.Debug($"{Character.Name}:AddMoney {Money}+={value}");
            Money += value;
        }

        public void ReduceMoney(int value)
        {
            Log.Debug($"{Character.Name}:ReduceMoney {Money}-={value}");
            Money -= value;
        }
    }
}
