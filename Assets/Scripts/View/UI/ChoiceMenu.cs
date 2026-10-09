#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using View;

namespace View.UI
{
    public class ChoiceMenu : MonoBehaviour, IMenu
    {
        private bool _canClose;
        public bool CanClose => _canClose;
        private readonly ReactiveProperty<int> _selectedIndex = new(-1);
        public ReadOnlyReactiveProperty<int> SelectedIndex => _selectedIndex;
        private readonly AsyncReactiveProperty<int> _choicedIndex = new(-1);
        internal IReadOnlyAsyncReactiveProperty<int> ChoicedIndex => _choicedIndex;
        [SerializeField] private TextMeshProUGUI _choiceText;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ChoiceButton _choiceButtonPrefab;
        [SerializeField] private SEManager _seManager;
        private readonly List<ChoiceButton> _buttons = new();
        private int _initialIndex;

        internal void SetCanCancel(bool canCancel) => _canClose = canCancel;

        internal void SetChoices(string? choiceText, int initialIndex, params string[] choices)
        {
            foreach (var button in _buttons)
            {
                Destroy(button.gameObject);
            }

            _buttons.Clear();

            if (choiceText != null)
                _choiceText.text = choiceText;
            else
                _choiceText.text = "";

            _initialIndex = Mathf.Clamp(initialIndex, 0, Mathf.Max(0, choices.Length - 1));
            _selectedIndex.Value = _initialIndex;
            foreach (var (choice, index) in choices.Index())
            {
                var button = Instantiate(_choiceButtonPrefab, _content);
                button.Construct(choice,
                    true,
                    () =>
                    {
                        _seManager.ChoiceCursorSE();
                        _selectedIndex.Value = index;
                    },
                    () =>
                    {
                        _seManager.ChoiceConfirmSE();
                        _choicedIndex.Value = index;
                    });
                _buttons.Add(button);
            }

            for (var i = 0; i < _buttons.Count; i++)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnUp = _buttons[(i - 1).WrapIndex(_buttons.Count)].GetComponent<Button>(),
                    selectOnDown = _buttons[(i + 1).WrapIndex(_buttons.Count)].GetComponent<Button>()
                };
                _buttons[i].GetComponent<Button>().navigation = nav;
                // 初期フォーカスは AutoSelecter を持つボタンに乗るため、defaultIndex のボタンだけ有効化する。
                _buttons[i].GetComponent<AutoSelecter>().enabled = i == _initialIndex;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
            LayoutRebuilder.ForceRebuildLayoutImmediate(_content);
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