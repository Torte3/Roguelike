using DG.Tweening;
using UnityEngine;

namespace View
{
    public class PopupIconMotion : MonoBehaviour
    {
        public float MoveY = 0.8f;
        public float DurationSeconds = 1f;

        private void Start()
        {
            var duration = Mathf.Max(0.1f, DurationSeconds);
            var sequence = DOTween.Sequence();
            sequence.Join(transform.DOMoveY(transform.position.y + MoveY, duration).SetEase(Ease.OutCubic));
            sequence.SetLink(gameObject);
            sequence.Play();
        }
    }
}
