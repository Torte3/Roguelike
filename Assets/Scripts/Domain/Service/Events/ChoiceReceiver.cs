#nullable enable
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character.Message;
using Domain.Model.Item;
using Domain.Model.Map;
using R3;
using Unity.Logging;

namespace Domain.Service.Events
{
    public class ChoiceReceiver
    {
        private readonly Subject<(ChoiceMessage? message, string[] choices, int? cancelChoiceIndex)> _onShownChoice = new();
        public Observable<(ChoiceMessage? message, string[] choices, int? cancelChoiceIndex)> OnShownChoice => _onShownChoice;
        private readonly Subject<(ChoiceMessage? message, (string choice, ChoiceMessage infoTitle, string info)[] choices, int defaultIndex, bool clearPreviousMenus)> _onShownChoiceWithInfo = new();
        public Observable<(ChoiceMessage? message, (string choice, ChoiceMessage infoTitle, string info)[] choices, int defaultIndex, bool clearPreviousMenus)> OnShownChoiceWithInfo => _onShownChoiceWithInfo;
        private readonly Subject<OnShownChoiceWithItemPreviewMessage> _onShownChoiceWithItemPreview = new();
        public Observable<OnShownChoiceWithItemPreviewMessage> OnShownChoiceWithItemPreview => _onShownChoiceWithItemPreview;
        private readonly AsyncReactiveProperty<int> _onReceivedChoicedIndex = new(-1);

        public UniTask<int> GetChoice(ChoiceMessage? message, params string[] choices) =>
            GetChoiceInternal(message, null, choices);

        public UniTask<int> GetChoice(ChoiceMessage? message, int cancelChoiceIndex, params string[] choices) =>
            GetChoiceInternal(message, cancelChoiceIndex, choices);

        private async UniTask<int> GetChoiceInternal(ChoiceMessage? message, int? cancelChoiceIndex, string[] choices)
        {
            Log.Debug($"[Menu]GetChoice: {message} {string.Join(", ", choices)}");
            ShowChoices(message, choices, cancelChoiceIndex);
            var index = await _onReceivedChoicedIndex.WaitAsync();
            Log.Debug($"[Menu]GetChoice: {index}");
            return index;
        }

        public UniTask<int> GetChoiceWithInfo(
            ChoiceMessage? message,
            int defaultIndex = 0,
            bool clearPreviousMenus = false,
            params (string choice, ChoiceMessage infoTitle, string info)[] choices) =>
            GetChoiceWithInfoInternal(message, choices, defaultIndex, clearPreviousMenus);

        private async UniTask<int> GetChoiceWithInfoInternal(
            ChoiceMessage? message,
            (string choice, ChoiceMessage infoTitle, string info)[] choices,
            int defaultIndex,
            bool clearPreviousMenus)
        {
            Log.Debug($"[Menu]GetChoice: {message} {string.Join(", ", choices.Select(c => c.choice))}");
            ShowChoicesWithInfo(message, choices, defaultIndex, clearPreviousMenus);
            var index = await _onReceivedChoicedIndex.WaitAsync();
            Log.Debug($"[Menu]GetChoice: {index}");
            return index;
        }

        public UniTask<int> GetChoiceWithItemPreview(ChoiceMessage? message, IMap map, params IItem[] items) =>
            GetChoiceWithItemPreviewInternal(message, map, null, items);

        public UniTask<int> GetChoiceWithItemPreview(
            ChoiceMessage? message,
            IMap map,
            int cancelChoiceIndex,
            params IItem[] items) =>
            GetChoiceWithItemPreviewInternal(message, map, cancelChoiceIndex, items);

        private async UniTask<int> GetChoiceWithItemPreviewInternal(
            ChoiceMessage? message,
            IMap map,
            int? cancelChoiceIndex,
            IItem[] items)
        {
            Log.Debug($"[Menu]GetChoiceWithItemPreview: {message} {string.Join(", ", items.Select(item => item.DebugName))}");
            _onShownChoiceWithItemPreview.OnNext(new OnShownChoiceWithItemPreviewMessage(message, map, items, cancelChoiceIndex));
            var index = await _onReceivedChoicedIndex.WaitAsync();
            Log.Debug($"[Menu]GetChoiceWithItemPreview: {index}");
            return index;
        }

        private void ShowChoicesWithInfo(ChoiceMessage? message, (string choice, ChoiceMessage infoTitle, string info)[] choices, int defaultIndex, bool clearPreviousMenus)
        {
            _onShownChoiceWithInfo.OnNext((message, choices, defaultIndex, clearPreviousMenus));
        }

        private void ShowChoices(ChoiceMessage? message, string[] choices, int? cancelChoiceIndex)
        {
            _onShownChoice.OnNext((message, choices, cancelChoiceIndex));
        }

        public void SetChoicedIndex(int index)
        {
            _onReceivedChoicedIndex.Value = index;
        }
    }
}
