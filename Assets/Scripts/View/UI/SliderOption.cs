using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    internal class SliderOption : MonoBehaviour, ISettingOption
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _valueText;
        public Selectable Selectable => _slider;

        public void SetData(string itemName, int min, int max, ReactiveProperty<int> value, ReadOnlyReactiveProperty<bool> isEnabled)
        {
            _text.SetText(itemName);
            _slider.minValue = min;
            _slider.maxValue = max;
            value.Subscribe(value =>
            {
                _slider.value = value;
                _valueText.SetText(value.ToString());
            }).AddTo(this);
            _slider.onValueChanged.AsObservable().Subscribe(v => value.Value = (int)v).AddTo(this);
            isEnabled.Subscribe(value => _slider.interactable = value).AddTo(this);
        }
    }
}