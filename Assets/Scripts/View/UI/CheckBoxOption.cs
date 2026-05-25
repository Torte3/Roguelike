using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI
{
    internal class CheckBoxOption : MonoBehaviour, ISettingOption
    {
        [SerializeField] private TMP_Text _text;
        [SerializeField] private Toggle _toggle;
        public Observable<bool> OnValueChanged => _toggle.onValueChanged.AsObservable();
        public Selectable Selectable => _toggle;

        public void SetData(string itemName, ReactiveProperty<bool> value, ReadOnlyReactiveProperty<bool> isEnabled)
        {
            _text.SetText(itemName);
            value.Subscribe(value => _toggle.isOn = value).AddTo(this);
            _toggle.onValueChanged.AsObservable().Subscribe(v => value.Value = v).AddTo(this);
            isEnabled.Subscribe(value => _toggle.interactable = value).AddTo(this);
        }
    }
}