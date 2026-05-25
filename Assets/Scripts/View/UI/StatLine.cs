using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    public class StatLine : MonoBehaviour
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Image _statBar;

        public void SetValue(float maxValue, float value)
        {
            _text.text = $"{value}/{maxValue}";
            _statBar.DOFillAmount(value / maxValue, 0.2f);
        }

        public void SetTextColor(Color color)
        {
            _text.color = color;
        }
    }
}