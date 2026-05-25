using Cysharp.Threading.Tasks;
using Domain.Model.Item;
using Domain.Service.Action;
using R3;
using Utilities;

namespace Domain.Service.Characters.Behavior
{
    public class CharacterControlInputReceiver
    {
        private ItemFocus _focus = new(0);
        private readonly Subject<Unit> _onActionRead = new();
        private readonly AsyncReactiveProperty<(Move action, bool isStarted)> _onMoveInputReceived = new((null, false));

        private readonly AsyncReactiveProperty<ItemFocus>
            _onUseItemActionReceived = new(new(0));

        private readonly AsyncReactiveProperty<ItemFocus> _onItemSelectConfirmReceived = new(new(0));

        private readonly AsyncReactiveProperty<ItemFocus> _onThrowItemActionReceived =
            new(new(0));

        private readonly AsyncReactiveProperty<ItemFocus> _onSwapItemActionReceived =
            new(new(0));

        private readonly AsyncReactiveProperty<Unit> _onDoNothingActionReceived = new(Unit.Default);

        private readonly AsyncReactiveProperty<ItemFocus> _onRenameItemActionReceived =
            new(new(0));

        private readonly AsyncReactiveProperty<Unit> _onFaceNearestCharacterActionReceived = new(Unit.Default);

        private readonly ReactiveProperty<bool> _enable = new(false);

        internal IReadOnlyAsyncReactiveProperty<(Move action, bool isStarted)> OnMoveInputReceived =>
            _onMoveInputReceived;

        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnUseItemActionReceived => _onUseItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnItemSelectConfirmReceived => _onItemSelectConfirmReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnThrowItemActionReceived => _onThrowItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnSwapItemActionReceived => _onSwapItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<Unit> OnDoNothingActionReceived => _onDoNothingActionReceived;
        internal IReadOnlyAsyncReactiveProperty<ItemFocus> OnRenameItemActionReceived => _onRenameItemActionReceived;
        internal IReadOnlyAsyncReactiveProperty<Unit> OnFaceNearestCharacterActionReceived => _onFaceNearestCharacterActionReceived;
        public Observable<Unit> OnActionRead => _onActionRead;
        public ReadOnlyReactiveProperty<bool> IsEnabled => _enable;

        public void SetMoveInput(Direction8 direction, bool isStarted)
        {
            if (_enable.CurrentValue)
                _onMoveInputReceived.Value = (new Move(direction), isStarted);
        }

        public void SetAttackInput()
        {
            if (_enable.CurrentValue)
                _onUseItemActionReceived.Value = _focus;
        }

        public void SetItemSelectConfirmInput()
        {
            if (_enable.CurrentValue)
                _onItemSelectConfirmReceived.Value = _focus;
        }

        public void SetThrowInput()
        {
            if (_enable.CurrentValue)
                _onThrowItemActionReceived.Value = _focus;
        }

        public void SetDropInput()
        {
            if (_enable.CurrentValue)
                _onSwapItemActionReceived.Value = _focus;
        }

        public void SetDoNothingInput()
        {
            if (_enable.CurrentValue)
                _onDoNothingActionReceived.Value = Unit.Default;
        }

        public void SetRenameInput()
        {
            if (_enable.CurrentValue)
                _onRenameItemActionReceived.Value = _focus;
        }

        public void SetFaceNearestCharacterInput()
        {
            if (_enable.CurrentValue)
                _onFaceNearestCharacterActionReceived.Value = Unit.Default;
        }

        public void SetItemFocus(ItemFocus focus)
        {
            if (_enable.CurrentValue)
                _focus = focus;
        }

        internal void ReadInput()
        {
            if (_enable.CurrentValue)
                _onActionRead.OnNext(Unit.Default);
        }

        public void Enable(bool enable)
        {
            _enable.Value = enable;
        }
    }
}