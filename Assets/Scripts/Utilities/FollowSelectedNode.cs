using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Utilities
{
    [RequireComponent(typeof(ScrollRect))]
    public class FollowSelectedNode : MonoBehaviour
    {
        private ScrollRect _scrollRect;
        private RectTransform _viewportRectransform;
        private Transform _contentTransform;
        [SerializeField] private RectTransform _nodePrefab;
        [SerializeField] private VerticalLayoutGroup _verticalLayoutGroup;
        [SerializeField] private float _scrollDuration = 0.2f;
        [SerializeField] private Ease _scrollEase = Ease.OutCubic;
        private Tween _scrollTween;

        void Start()
        {
            _scrollRect = GetComponent<ScrollRect>();
            _viewportRectransform = _scrollRect.viewport;
            _contentTransform = _scrollRect.content;
            Observable.EveryUpdate()
                .Select(_ => EventSystem.current != null ? EventSystem.current.currentSelectedGameObject : null)
                .DistinctUntilChanged()
                .Select(go => GetNodeIndex(go != null ? go.transform : null))
                .Where(index => index >= 0)
                .DistinctUntilChanged()
                .Subscribe(index => Scroll(index))
                .AddTo(this);
        }

        private int GetNodeIndex(Transform target)
        {
            if (target == null || _contentTransform == null) return -1;

            var current = target;
            while (current != null && current != _contentTransform)
            {
                if (current.parent == _contentTransform)
                {
                    return current.GetSiblingIndex();
                }
                current = current.parent;
            }

            return -1;
        }

        void Scroll(int nodeIndex)
        {
            var p = 1.0f - _scrollRect.verticalNormalizedPosition;
            var nodeCount = _contentTransform.childCount;
            var viewportSize = _viewportRectransform.rect.height;
            var harlViewport = viewportSize * 0.5f;

            var nodeSize = _nodePrefab.rect.height + _verticalLayoutGroup.spacing;

            var scrollableHeight = nodeSize * nodeCount - viewportSize;
            if (scrollableHeight <= 0f) return;

            var centerPosition = scrollableHeight * p + harlViewport;
            var topPosition = centerPosition - harlViewport;
            var bottomPosition = centerPosition + harlViewport;

            var nodeTop = nodeSize * nodeIndex;
            var nodeBottom = nodeSize * (nodeIndex + 1);

            float? target = null;

            if (topPosition > nodeTop)
            {
                var newP = nodeTop / scrollableHeight;
                target = 1.0f - newP;
            }
            else if (nodeBottom > bottomPosition)
            {
                var newP = (nodeBottom - viewportSize) / scrollableHeight;
                target = 1.0f - newP;
            }

            if (target.HasValue)
            {
                if (_scrollTween != null && _scrollTween.IsActive()) _scrollTween.Kill();
                _scrollTween = DOTween.To(
                    () => _scrollRect.verticalNormalizedPosition,
                    v => _scrollRect.verticalNormalizedPosition = v,
                    target.Value,
                    _scrollDuration
                ).SetEase(_scrollEase);
            }
        }

        void OnDestroy()
        {
            if (_scrollTween != null && _scrollTween.IsActive()) _scrollTween.Kill();
        }
    }
}