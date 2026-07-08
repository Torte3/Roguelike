using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace View.UI.Options
{
    internal class CheckBoxText : MonoBehaviour
    {
        [SerializeField] private Toggle _checkBox;
        [SerializeField] private TMP_Text _text;

        private void Start()
        {
            _text.SetText(GetText(_checkBox.isOn));
            _checkBox.onValueChanged.AsObservable().Subscribe(value => { _text.SetText(GetText(value)); }).AddTo(this);
        }

        private string GetText(bool value)
        {
            return value ? "有効" : "無効";
        }
    }
}