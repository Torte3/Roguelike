using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utilities
{
    [RequireComponent(typeof(ScrollRect))]
    public class ScrollRectNavigator : MonoBehaviour, IMoveHandler, IScrollHandler
    {
        [SerializeField] private int _scrollLinesPerStep = 3;
        [SerializeField] private float _lineHeight;
        [SerializeField] private float _scrollDuration = 0.2f;
        [SerializeField] private Ease _scrollEase = Ease.OutCubic;
        private ScrollRect _scrollRect = null!;
        private Tween _scrollTween;

        void Awake()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _scrollRect.horizontal = false;
        }

        void OnDestroy()
        {
            if (_scrollTween != null && _scrollTween.IsActive())
                _scrollTween.Kill();
        }

        public void Configure(int scrollLinesPerStep, float lineHeight)
        {
            _scrollLinesPerStep = scrollLinesPerStep;
            _lineHeight = lineHeight;
        }

        public void OnMove(AxisEventData eventData)
        {
            switch (eventData.moveDir)
            {
                case MoveDirection.Up:
                    ScrollByLines(_scrollLinesPerStep);
                    break;
                case MoveDirection.Down:
                    ScrollByLines(-_scrollLinesPerStep);
                    break;
            }
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (Mathf.Approximately(eventData.scrollDelta.y, 0f))
                return;

            ScrollByLines(Mathf.Sign(eventData.scrollDelta.y) * _scrollLinesPerStep);
        }

        private void ScrollByLines(float lineCount)
        {
            var viewportHeight = _scrollRect.viewport.rect.height;
            var contentHeight = _scrollRect.content.rect.height;
            var scrollable = contentHeight - viewportHeight;
            if (scrollable <= 0f || _lineHeight <= 0f)
                return;

            var delta = lineCount * _lineHeight / scrollable;
            var target = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition + delta);

            if (_scrollTween != null && _scrollTween.IsActive())
                _scrollTween.Kill();

            _scrollTween = DOTween.To(
                () => _scrollRect.verticalNormalizedPosition,
                v => _scrollRect.verticalNormalizedPosition = v,
                target,
                _scrollDuration
            ).SetEase(_scrollEase);
        }
    }
}
