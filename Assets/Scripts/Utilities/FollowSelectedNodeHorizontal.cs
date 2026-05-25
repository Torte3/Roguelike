using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace UI
{
    [RequireComponent(typeof(ScrollRect))]
    public class FollowSelectedNodeHorizontal : MonoBehaviour
    {
        private ScrollRect _scrollRect;
        private RectTransform _viewportRectransform;
        private Transform _contentTransform;
        [SerializeField] private RectTransform _nodePrefab;
        [SerializeField] private HorizontalLayoutGroup _horizontalLayoutGroup;
        [SerializeField] private float _scrollDuration = 0.2f;
        [SerializeField] private Ease _scrollEase = Ease.OutCubic;
        private Tween _scrollTween;

        private void Start()
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

        public void Scroll(int nodeIndex)
        {
            float spacing = _horizontalLayoutGroup.spacing;
            float p = _scrollRect.horizontalNormalizedPosition;
            int nodeCount = _contentTransform.childCount;
            float viewportSize = _viewportRectransform.rect.width;
            float halfViewport = viewportSize * 0.5f;

            float nodeSize = _nodePrefab.rect.width + spacing;

            float scrollableWidth = nodeSize * nodeCount - viewportSize;
            if (scrollableWidth <= 0f) return;

            float centerPosition = scrollableWidth * p + halfViewport;
            float leftPosition = centerPosition - halfViewport;
            float rightPosition = centerPosition + halfViewport;

            float nodeCenterPosition = nodeSize * nodeIndex + nodeSize / 2.0f;

            float nodeLeftPosition = nodeCenterPosition - nodeSize / 2;
            float nodeRightPosition = nodeCenterPosition + nodeSize / 2;

            float? target = null;

            if (leftPosition > nodeLeftPosition)
            {
                float newP = (nodeSize * nodeIndex) / scrollableWidth;
                target = newP;
            }

            if (nodeRightPosition > rightPosition)
            {
                float newP = (((nodeSize * (nodeIndex + 1)) + spacing - viewportSize)) / scrollableWidth;
                target = newP;
            }

            if (target.HasValue)
            {
                if (_scrollTween != null && _scrollTween.IsActive()) _scrollTween.Kill();
                _scrollTween = DOTween.To(
                    () => _scrollRect.horizontalNormalizedPosition,
                    v => _scrollRect.horizontalNormalizedPosition = v,
                    target.Value,
                    _scrollDuration
                ).SetEase(_scrollEase);
            }
        }

        private void OnDestroy()
        {
            if (_scrollTween != null && _scrollTween.IsActive()) _scrollTween.Kill();
        }
    }
}
