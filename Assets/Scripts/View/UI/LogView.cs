using Configuration;
using DG.Tweening;
using TMPro;
using Unity.Logging;
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
        public void AddLog(string message, bool appendToPrevious)
        {
            Log.Info($"[GameLog]{message}");
            if (appendToPrevious && TryAppendToLastLog(message))
            {
                return;
            }

            var logText = Instantiate(_logTextPrefab, _content);
            logText.text = message;
            // TMPのpreferred width確定前にレイアウトを読むと位置がずれるため先に確定させる。
            logText.ForceMeshUpdate();
            logText.gameObject.AddComponent<LifeTimer>().LifeTimeMilliseconds = Settings.GlobalSettings.LogShownMilliSeconds.CurrentValue;
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content as RectTransform);
            PlaySlideIn(logText.rectTransform);
        }

        // 新規ログをレイアウト確定位置の左からスライドインさせる。
        private void PlaySlideIn(RectTransform rectTransform)
        {
            var position = rectTransform.anchoredPosition;
            rectTransform.anchoredPosition = new Vector2(position.x - _slideInDistance, position.y);
            rectTransform.DOAnchorPosX(position.x, _slideInDuration).SetEase(_slideInEase).SetLink(rectTransform.gameObject);
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