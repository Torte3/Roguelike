using DG.Tweening;
using R3;
using UnityEngine;
using Utilities;

namespace View
{
    [RequireComponent(typeof(SpriteView))]
    public class EntityView : MonoBehaviour
    {
        private int ThrowMilliseconds = 1000;
        private int MoveMilliseconds = 1000;
        private int DashMilliseconds = 1000;
        private ReadOnlyReactiveProperty<bool> _isDash;
        private SpriteView _view;
        private bool _isVisible => _view.GetVisibility();
        private readonly SerialDisposable _disposable = new();
        public bool IsMoving { get; private set; }

        public void Construct(ReadOnlyReactiveProperty<bool> isDash)
        {
            _isDash = isDash;
            _view = GetComponent<SpriteView>();
        }

        public void SetThrowMilliseconds(int throwMilliseconds)
        {
            ThrowMilliseconds = throwMilliseconds;
        }

        public void SetMoveMilliseconds(int moveMilliseconds)
        {
            MoveMilliseconds = moveMilliseconds;
        }

        public void SetDashMilliseconds(int dashMilliseconds)
        {
            DashMilliseconds = dashMilliseconds;
        }

        public void SetPosition(Vector2 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
        }

        public void Teleport(Vector2Int position)
        {
            _disposable.Disposable = null;
            transform.DOKill();
            SetPosition(position);
            IsMoving = false;
        }

        public void Move(Vector2Int destination, Direction8 direction, bool isThrown)
        {
            _disposable.Disposable = null;
            transform.DOKill();

            if (_isVisible)
            {
                SetPosition(destination - direction.Vector());

                var totalDuration = 1f;
                if (isThrown)
                    totalDuration = ThrowMilliseconds / 1000f;
                else if (_isDash.CurrentValue)
                    totalDuration = DashMilliseconds / 1000f;
                else
                    totalDuration = MoveMilliseconds / 1000f;

                IsMoving = true;

                transform.DOMove(new Vector3(destination.x, destination.y, transform.position.z), totalDuration)
                    .SetEase(Ease.Linear)
                    .OnComplete(() => { IsMoving = false; });
            }
            else
            {
                SetPosition(destination);
                IsMoving = false;
            }
        }
    }
}