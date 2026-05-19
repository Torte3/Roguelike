#nullable enable
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;

namespace View.UI
{
    public class TextInputMenu : MonoBehaviour, IMenu
    {
        private bool _canCancel;
        public bool CanClose => _canCancel;
        [SerializeField] private TMP_InputField _inputField;
        private readonly AsyncReactiveProperty<string> _text = new("");
        public IReadOnlyAsyncReactiveProperty<string> Text => _text;

        private void Awake()
        {
            _inputField.onEndEdit.AddListener(text => _text.Value = text);
        }

        public void SetCanCancel(bool canCancel) => _canCancel = canCancel;

        public void Show()
        {
            _inputField.text = "";
            gameObject.SetActive(true);
            _inputField.ActivateInputField();
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }
    }
}