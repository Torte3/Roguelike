using System;
using DG.Tweening;
using UnityEngine;

namespace View
{
    [RequireComponent(typeof(SpriteView))]
    public class EntityView : MonoBehaviour
    {
        internal bool IsMoving { get; private set; }

        public void SetPosition(Vector2 position)
        {
            transform.DOKill();
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            IsMoving = false;
        }

        public void MoveTo(Vector2 destination, float duration, Action onComplete = null)
        {
            transform.DOKill();
            IsMoving = true;
            transform.DOMove(new Vector3(destination.x, destination.y, transform.position.z), duration)
                .SetEase(Ease.Linear)
                .SetLink(gameObject)
                .OnComplete(() =>
                {
                    IsMoving = false;
                    onComplete?.Invoke();
                });
        }
    }
}
