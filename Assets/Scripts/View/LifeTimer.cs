using System;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using Unity.Logging;
using UnityEngine;
using UnityEngine.UI;

namespace View
{
    internal class LifeTimer : MonoBehaviour
    {
        public int LifeTimeMilliseconds = 1000;
        public float FadeOutDuration = 0.3f;

        private async void Start()
        {
            try
            {
                // フェードアウト開始時間を計算
                var fadeStartTime = LifeTimeMilliseconds - (int)(FadeOutDuration * 1000);
                fadeStartTime = Mathf.Max(0, fadeStartTime);
                
                // メインの待機時間
                await UniTask.Delay(fadeStartTime, cancellationToken: destroyCancellationToken);
                
                // フェードアウト開始
                StartFadeOut();
                
                // フェードアウト完了まで待機
                await UniTask.Delay((int)(FadeOutDuration * 1000), cancellationToken: destroyCancellationToken);
                
                Destroy(gameObject);
            }
            catch (OperationCanceledException e)
            {
                Log.Debug($"OperationCanceledException: {e}");
            }
        }

        private void StartFadeOut()
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.DOFade(0f, FadeOutDuration).SetEase(Ease.OutQuad);
                return;
            }
            
            var graphic = GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.DOFade(0f, FadeOutDuration).SetEase(Ease.OutQuad);
                return;
            }
            
            var spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.DOFade(0f, FadeOutDuration).SetEase(Ease.OutQuad);
                return;
            }

            Log.Error($"DOFade method not found in {gameObject.name}");
        }
    }
}