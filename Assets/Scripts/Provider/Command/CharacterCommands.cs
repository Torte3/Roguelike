#nullable enable
using System;
using System.Linq;
using Domain.Model.Character;
using Game;
using IngameDebugConsole;
using Unity.Logging;
using UnityEngine;
using VContainer;
using R3;

namespace Provider
{
    public class CharacterCommands
    {
        private readonly World _world;

        [Inject]
        public CharacterCommands(World world)
        {
            _world = world;

            DebugLogConsole.AddCommandInstance(
                "findCharacter",
                "指定した位置にいるキャラクターのIDを取得します。",
                "FindCharacter",
                this);
            DebugLogConsole.AddCommandInstance(
                "findAllCharacters",
                "すべてのキャラクターのIDを取得します。",
                "FindAllCharacters",
                this);
            DebugLogConsole.AddCommandInstance(
                "getCharacterJson",
                "指定した位置にいるキャラクターのJsonを取得します。",
                "GetCharacterJson",
                this);
        }

        private void ShowCharacter(ICharacter character)
        {
            var info = $"{character.GetNameIgnoreVisibility(_world.CurrentMap.Player)}\n"
                       + $"Id: {character.Entity.Id}\n"
                       + $"Position: {character.Entity.CurrentPosition}\n"
                       + $"CharacterType: {character.CharacterType.SubtypeName()}";
            Log.Info(info);
        }

        private void ShowCharacterJson(ICharacter character)
        {
            Log.Info(JsonUtility.ToJson(character.Serialize(), true));
        }

        private void FindCharacter(Vector2Int position)
        {
            var character =
                _world.CurrentMap.Characters.FirstOrDefault(character =>
                    character.Entity.CurrentPosition == position);
            if (character != null)
            {
                ShowCharacter(character);
            }
            else
            {
                Log.Error("指定された位置にキャラクターが見つかりません。");
            }
        }

        private void FindAllCharacters()
        {
            var characters = _world.CurrentMap.Characters;
            foreach (var character in characters)
            {
                ShowCharacter(character);
            }
        }

        private void GetCharacterJson(string target)
        {
            try
            {
                var character = CommandUtilities.GetTarget(target, _world.CurrentMap);
                ShowCharacterJson(character);
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }
}