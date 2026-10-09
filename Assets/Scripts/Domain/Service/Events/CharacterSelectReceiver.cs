#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using R3;
using Unity.Logging;

namespace Domain.Service.Events
{
    public class CharacterSelectReceiver
    {
        private readonly Subject<List<(string name, string textureName, UnlockCondition unlock, string info)>> _onShownChoice = new();
        public Observable<List<(string name, string textureName, UnlockCondition unlock, string info)>> OnShownChoice => _onShownChoice;
        private readonly AsyncReactiveProperty<int?> _onReceivedChoicedIndex = new(null);

        public async UniTask<int?> GetCharacter(List<(string name, string textureName, UnlockCondition unlock, string info)> characters)
        {
            Log.Debug($"[Menu]GetCharacter");
            ShowCharacter(characters);
            var index = await _onReceivedChoicedIndex.WaitAsync();
            Log.Debug($"[Menu]GetCharacter: {index}");
            return index;
        }

        private void ShowCharacter(List<(string name, string textureName, UnlockCondition unlock, string info)> characters)
        {
            _onShownChoice.OnNext(characters);
        }

        public void SetChoicedIndex(int? index)
        {
            _onReceivedChoicedIndex.Value = index;
        }
    }
}