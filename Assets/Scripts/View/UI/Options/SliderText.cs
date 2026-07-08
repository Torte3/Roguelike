using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Options
{
    internal class SliderText : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _text;

        private void Start()
        {
            _text.SetText(_slider.value.ToString());
            _slider.onValueChanged.AsObservable().Subscribe(value => { _text.SetText(value.ToString()); }).AddTo(this);
        }
    }
}