#nullable enable
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Character.Message;
using Domain.Model.Item;
using Domain.Model.Map;
using R3;
using Unity.Logging;

namespace Domain.Service.Events
{
    public class ChoiceReceiver
    {
        private readonly Subject<(string? text, string[] choices, int? cancelChoiceIndex)> _onShownChoice = new();
        public Observable<(string? text, string[] choices, int? cancelChoiceIndex)> OnShownChoice => _onShownChoice;
        private readonly Subject<(string? text, (string choice, string infoTitle, string info)[] choices, int? cancelChoiceIndex)> _onShownChoiceWithInfo = new();
        public Observable<(string? text, (string choice, string infoTitle, string info)[] choices, int? cancelChoiceIndex)> OnShownChoiceWithInfo => _onShownChoiceWithInfo;
        private readonly Subject<OnShownChoiceWithItemPreviewMessage> _onShownChoiceWithItemPreview = new();
        public Observable<OnShownChoiceWithItemPreviewMessage> OnShownChoiceWithItemPreview => _onShownChoiceWithItemPreview;
        private readonly AsyncReactiveProperty<int> _onReceivedChoicedIndex = new(-1);

        public UniTask<int> GetChoice(string? text, params string[] choices) =>
            GetChoiceInternal(text, null, choices);

        public UniTask<int> GetChoice(string? text, int cancelChoiceIndex, params string[] choices) =>
            GetChoiceInternal(text, cancelChoiceIndex, choices);

        private async UniTask<int> GetChoiceInternal(string? text, int? cancelChoiceIndex, string[] choices)
        {
            Log.Debug($"[Menu]GetChoice: {text} {string.Join(", ", choices)}");
            ShowChoices(text, choices, cancelChoiceIndex);
            var index = await _onReceivedChoicedIndex.WaitAsync();
            Log.Debug($"[Menu]GetChoice: {index}");
            return index;
        }

        public UniTask<int> GetChoiceWithInfo(string? text, params (string choice, string infoTitle, string info)[] choices) =>
            GetChoiceWithInfoInternal(text, null, choices);

        public UniTask<int> GetChoiceWithInfo(
            string? text,
            int cancelChoiceIndex,
            params (string choice, string infoTitle, string info)[] choices) =>
            GetChoiceWithInfoInternal(text, cancelChoiceIndex, choices);

        private async UniTask<int> GetChoiceWithInfoInternal(
            string? text,
            int? cancelChoiceIndex,
            (string choice, string infoTitle, string info)[] choices)
        {
            Log.Debug($"[Menu]GetChoice: {text} {string.Join(", ", choices.Select(c => c.choice))}");
            ShowChoicesWithInfo(text, choices, cancelChoiceIndex);
            var index = await _onReceivedChoicedIndex.WaitAsync();
            Log.Debug($"[Menu]GetChoice: {index}");
            return index;
        }

        public UniTask<int> GetChoiceWithItemPreview(string? text, IMap map, params (string choice, IItem item)[] choices) =>
            GetChoiceWithItemPreviewInternal(text, map, null, choices);

        public UniTask<int> GetChoiceWithItemPreview(
            string? text,
            IMap map,
            int cancelChoiceIndex,
            params (string choice, IItem item)[] choices) =>
            GetChoiceWithItemPreviewInternal(text, map, cancelChoiceIndex, choices);

        private async UniTask<int> GetChoiceWithItemPreviewInternal(
            string? text,
            IMap map,
            int? cancelChoiceIndex,
            (string choice, IItem item)[] choices)
        {
            Log.Debug($"[Menu]GetChoiceWithItemPreview: {text} {string.Join(", ", choices.Select(c => c.choice))}");
            _onShownChoiceWithItemPreview.OnNext(new OnShownChoiceWithItemPreviewMessage(text, map, choices, cancelChoiceIndex));
            var index = await _onReceivedChoicedIndex.WaitAsync();
            Log.Debug($"[Menu]GetChoiceWithItemPreview: {index}");
            return index;
        }

        private void ShowChoicesWithInfo(string? text, (string choice, string infoTitle, string info)[] choices, int? cancelChoiceIndex)
        {
            _onShownChoiceWithInfo.OnNext((text, choices, cancelChoiceIndex));
        }

        private void ShowChoices(string? text, string[] choices, int? cancelChoiceIndex)
        {
            _onShownChoice.OnNext((text, choices, cancelChoiceIndex));
        }

        public void SetChoicedIndex(int index)
        {
            _onReceivedChoicedIndex.Value = index;
        }
    }
}
