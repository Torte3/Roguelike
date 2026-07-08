#nullable enable
using Provider.Input;
using R3;
using VContainer;
using View.UI.HUD;

namespace Provider.Presenters
{
    /// <summary>
    /// 入力デバイス（キーボード/コントローラー）の変化を検出し、キーヒント表示を切り替える。
    /// </summary>
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
