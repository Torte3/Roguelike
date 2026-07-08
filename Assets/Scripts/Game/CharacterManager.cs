#nullable enable
using System;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Characters;
using Domain.Service.Characters.Behavior;
using Domain.Service.Rooms;
using ObservableCollections;
using Utilities;

namespace Game
{
    internal sealed class CharacterManager : IDisposable
    {
        private readonly ObservableList<ICharacter> _characters = new();

        public CharacterManager(PlayerMemento playerData, CharacterControlInputReceiver receiver, IGameManager gameManager, IMap map)
        {
            _characters.SubscribeIncludingCurrentObservables(
                character => character.Entity.OnDestroyed,
                (character, _) =>
                {
                    RemoveCharacter(character);
                }
            );

            Player = CharacterFactory.CreatePlayer(playerData, receiver, gameManager, map);

            if (!Player.Character.IsDead)
            {
                AddCharacter(Player.Character);
            }
        }

        public readonly IPlayer Player;

        public IObservableCollection<ICharacter> Characters => _characters;

        public void Dispose()
        {
            _characters.ForEach(character => character.Dispose());
        }

        public void AddCharacter(ICharacter character)
        {
            _characters.Add(character);
        }

        private void RemoveCharacter(ICharacter character)
        {
            _characters.Remove(character);
        }

        public ICharacter SpawnAlly(CharacterMemento data, IMap map)
        {
            var behavior = new EnemyBehavior(data.Behavior, map.Id);
            var character = CharacterFactory.CreateCharacter(data, behavior, map);
            AddCharacter(character);
            character.AddEvent(new Ally(character, behavior));
            return character;
        }
    }
}