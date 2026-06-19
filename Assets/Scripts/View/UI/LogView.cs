using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class LogView : MonoBehaviour
    {
        [SerializeField] private Transform _content;
        [SerializeField] private TMP_Text _logTextPrefab;
        [Header("Slide-in")]
        [SerializeField] private float _slideInDistance = 60f;
        [SerializeField] private float _slideInDuration = 0.2f;
        [SerializeField] private Ease _slideInEase = Ease.OutCubic;
        private int _logShownMilliSeconds = 3000;

        public void SetLogShownMilliSeconds(int milliSeconds)
        {
            _logShownMilliSeconds = milliSeconds;
        }

        public void AddLog(string message, bool appendToPrevious)
        {
            if (appendToPrevious && TryAppendToLastLog(message))
            {
                return;
            }

            var logText = Instantiate(_logTextPrefab, _content);
            logText.text = message;
            logText.ForceMeshUpdate();
            logText.gameObject.AddComponent<LifeTimer>().LifeTimeMilliseconds = _logShownMilliSeconds;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content as RectTransform);
            PlaySlideIn(logText.rectTransform);
        }

        private void PlaySlideIn(RectTransform rectTransform)
        {
            var position = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(position.x - _slideInDistance, position.y);
            rectTransform.DOAnchorPosX(position.x, _slideInDuration).SetEase(_slideInEase);
        }

        public void Clear()
        {
            foreach (var logText in _content.GetComponentsInChildren<TMP_Text>())
            {
                Destroy(logText.gameObject);
            }
        }

        private bool TryAppendToLastLog(string message)
        {
            var childCount = _content.childCount;
            if (childCount <= 0)
            {
                return false;
            }

            var lastLogText = _content.GetChild(childCount - 1).GetComponent<TMP_Text>();
            if (lastLogText == null)
            {
                return false;
            }

            lastLogText.text += message;
            LayoutRebuilder.ForceRebuildLayoutImmediate((RectTransform)lastLogText.transform);
            return true;
        }
    }
}