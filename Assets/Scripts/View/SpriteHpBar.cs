using DG.Tweening;
using UnityEngine;

namespace View
{
    public class SpriteHpBar : MonoBehaviour
    {
        [SerializeField] private Transform _statBar;
        
        public void Show()
        {
            gameObject.SetActive(true);
        }
        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void SetValue(float maxValue, float value)
        {
            _statBar.DOLocalMove(new Vector3(-0.5f + value / maxValue / 2, _statBar.localPosition.y,
                _statBar.localPosition.z), 0.2f);
            _statBar.DOScale(new Vector3(value / maxValue, _statBar.localScale.y, _statBar.localScale.z), 0.2f);
        }
    }
}