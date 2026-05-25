using DG.Tweening;
using UnityEngine;

namespace View
{
    public class DamageText : MonoBehaviour
    {
        private void Start()
        {
            var sequence = DOTween.Sequence();
            sequence.Join(transform.DOMoveX(Random.Range(-1, 1), 1).SetRelative().SetEase(Ease.OutQuart));
            sequence.Join(transform.DOMoveY(-1, 0.5f).SetRelative().SetEase(Ease.OutBounce));
            sequence.Play();
        }
    }
}