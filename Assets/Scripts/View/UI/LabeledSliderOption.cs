using System.Collections.Generic;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    internal class LabeledSliderOption : MonoBehaviour, ISettingOption
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Slider _slider;
        [SerializeField] private TMP_Text _valueText;
        public Selectable Selectable => _slider;

        public void SetData(string itemName, IReadOnlyList<(int Value, string Label)> options, ReactiveProperty<int> index, ReadOnlyReactiveProperty<bool> isEnabled)
        {
            _text.SetText(itemName);
            _slider.minValue = 0;
            _slider.maxValue = options.Count - 1;
            index.Subscribe(value =>
            {
                _slider.value = value;
                _valueText.SetText(options[value].Label);
            }).AddTo(this);
            _slider.onValueChanged.AsObservable().Subscribe(v => index.Value = (int)v).AddTo(this);
            isEnabled.Subscribe(value => _slider.interactable = value).AddTo(this);
        }
    }
}