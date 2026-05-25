#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.Setting;
using Domain.Service.Characters;
using Domain.Service.Characters.Behavior;
using Domain.Service.Rooms;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Game
{
    public sealed class CharacterManager : IDisposable
    {
        private readonly ObservableList<ICharacter> _characters = new();
        private HashSet<Vector2Int> _allCharacterPositions = new();

        public CharacterManager(PlayerMemento playerData, CharacterControlInputReceiver receiver, IGameManager gameManager, IMap map)
        {
            _characters.ObserveCountChanged().Subscribe(_ => SetAllCharacterPosition());
            _characters.SubscribeIncludingCurrentObservables(
                character => character.Entity.Position,
                (character, _) => SetAllCharacterPosition()
            );
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

        public void RemoveCharacter(ICharacter character)
        {
            _characters.Remove(character);
        }

        public ICharacter SpawnCharacter(CharacterMemento data, IGameManager gameManager, IMap map)
        {
            var character = CharacterFactory.CreateCharacter(data, new EnemyBehavior(data.Behavior, map.Id), gameManager, map);
            AddCharacter(character);
            return character;
        }

        public ICharacter SpawnAlly(CharacterMemento data, IGameManager gameManager, IMap map)
        {
            var behavior = new EnemyBehavior(data.Behavior, map.Id);
            var character = CharacterFactory.CreateCharacter(data, behavior, gameManager, map);
            AddCharacter(character);
            character.AddEvent(new Ally(character, behavior));
            return character;
        }

        public HashSet<Vector2Int> GetAllCharacterPositions()
        {
            return _allCharacterPositions;
        }

        private void SetAllCharacterPosition()
        {
            _allCharacterPositions = _characters.Positions().ToHashSet();
        }
    }
}