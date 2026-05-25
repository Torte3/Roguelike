#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace View.UI
{
    public class StatisticsMenu : MonoBehaviour, IMenu
    {
        public bool CanClose => true;
        [SerializeField] private TMP_Text _statisticsText = null!;
        [SerializeField] private ScrollRect _scrollRect = null!;
        [SerializeField] private int _scrollLinesPerStep = 3;
        private Selectable _scrollFocus = null!;
        private bool _initialized;

        void Awake()
        {
            EnsureInitialized();
        }

        public void SetText(string text)
        {
            EnsureInitialized();
            _statisticsText.text = text;
            RebuildScrollContent();
            ResetScrollPosition();
        }

        public void Show()
        {
            EnsureInitialized();
            gameObject.SetActive(true);
            RebuildScrollContent();
            ResetScrollPosition();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Enable()
        {
            EnsureInitialized();
            if (_scrollFocus == null)
                return;
            EventSystem.current.SetSelectedGameObject(_scrollFocus.gameObject);
        }

        public void Disable()
        {
        }

        private void EnsureInitialized()
        {
            if (_initialized)
                return;
            EnsureTextFitsContent();
            EnsureScrollNavigation();
            _initialized = true;
        }

        private void EnsureTextFitsContent()
        {
            _statisticsText.raycastTarget = false;
            if (_statisticsText.TryGetComponent<ContentSizeFitter>(out _))
                return;

            var fitter = _statisticsText.gameObject.AddComponent<ContentSizeFitter>();
            fitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        }

        private void EnsureScrollNavigation()
        {
            if (_scrollRect == null)
                _scrollRect = GetComponentInChildren<ScrollRect>(true);
            if (_scrollRect == null)
                return;

            if (!_scrollRect.TryGetComponent<ScrollRectNavigator>(out var navigator))
                navigator = _scrollRect.gameObject.AddComponent<ScrollRectNavigator>();
            navigator.Configure(_scrollLinesPerStep, GetLineHeight());

            if (!_scrollRect.TryGetComponent<Button>(out var button))
            {
                button = _scrollRect.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
            }

            var navigation = button.navigation;
            navigation.mode = Navigation.Mode.None;
            button.navigation = navigation;
            _scrollFocus = button;
            _scrollRect.horizontal = false;
        }

        private float GetLineHeight()
        {
            _statisticsText.ForceMeshUpdate();
            if (_statisticsText.textInfo.lineCount > 0)
                return _statisticsText.textInfo.lineInfo[0].lineHeight;
            return _statisticsText.fontSize * (1f + _statisticsText.lineSpacing);
        }

        private void RebuildScrollContent()
        {
            if (_scrollRect == null)
                return;

            Canvas.ForceUpdateCanvases();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_statisticsText.rectTransform);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_scrollRect.content);

            if (_scrollRect.TryGetComponent<ScrollRectNavigator>(out var navigator))
                navigator.Configure(_scrollLinesPerStep, GetLineHeight());
        }

        private void ResetScrollPosition()
        {
            if (_scrollRect == null)
                return;
            _scrollRect.verticalNormalizedPosition = 1f;
        }
    }
}
