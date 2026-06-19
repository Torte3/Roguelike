#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model;
using R3;

namespace Domain.Service.Events
{
    public class TutorialReceiver
    {
        private readonly Subject<TutorialType> _onShown = new();
        public Observable<TutorialType> OnShown => _onShown;
        private readonly Subject<Unit> _onHidden = new();
        public Observable<Unit> OnHidden => _onHidden;
        private readonly AsyncReactiveProperty<Unit> _onAdvanced = new(Unit.Default);

        public async UniTask Show(TutorialType type)
        {
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
