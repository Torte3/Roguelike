#nullable enable
using Provider.Input;
using R3;
using VContainer;
using View.UI;

namespace Provider
{
    public class KeyHintPresenter
    {
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public KeyHintPresenter(InputReceiver receiver, KeyHintView keyHintView)
        {
            receiver.IsUsingKeyboard
                .Subscribe(keyHintView.SetUsingKeyboard)
                .AddTo(_disposables);
        }

        ~KeyHintPresenter()
        {
            _disposables.Dispose();
        }
    }
}
