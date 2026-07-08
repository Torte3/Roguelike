#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model;
using R3;

namespace Domain.Service.Events.Receivers
{
    // チュートリアル表示の待受。ChoiceReceiver と同じく、表示を通知してから
    // 決定キー（Advance）が押されるまで待つ。ボタンは持たない。
    public class TutorialReceiver
    {
        private readonly Subject<TutorialType> _onShown = new();
        public Observable<TutorialType> OnShown => _onShown;
        private readonly Subject<Unit> _onHidden = new();
        public Observable<Unit> OnHidden => _onHidden;
        private readonly AsyncReactiveProperty<Unit> _onAdvanced = new(Unit.Default);
        private Func<UniTask> _waitUntilReady = () => UniTask.CompletedTask;

        public void SetReadyWaiter(Func<UniTask> waitUntilReady)
        {
            _waitUntilReady = waitUntilReady ?? (() => UniTask.CompletedTask);
        }

        // 指定種類のチュートリアルを表示し、決定キーが押される（Advance）まで待つ。
        public async UniTask Show(TutorialType type)
        {
            await _waitUntilReady();
            _onShown.OnNext(type);
            await _onAdvanced.WaitAsync();
            _onHidden.OnNext(Unit.Default);
        }

        public void Advance()
        {
            _onAdvanced.Value = Unit.Default;
        }
    }
}
