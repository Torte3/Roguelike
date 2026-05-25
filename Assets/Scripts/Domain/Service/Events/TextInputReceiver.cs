#nullable enable
using Cysharp.Threading.Tasks;
using R3;
using Unity.Logging;

namespace Domain.Service.Events
{
    public class TextInputReceiver
    {
        private readonly Subject<bool> _onShownTextInput = new();
        public Observable<bool> OnShownTextInput => _onShownTextInput;
        private readonly AsyncReactiveProperty<string?> _onReceivedTextInput = new(null);

        public async UniTask<string?> GetTextInput(bool canCancel = false)
        {
            Log.Debug("GetTextInput");
            ShowTextInput(canCancel);
            var text = await _onReceivedTextInput.WaitAsync();
            Log.Debug($"GetTextInput: {text}");
            return text;
        }

        private void ShowTextInput(bool canCancel)
        {
            _onShownTextInput.OnNext(canCancel);
        }

        public void SetTextInput(string? text)
        {
            _onReceivedTextInput.Value = text;
        }
    }
}
