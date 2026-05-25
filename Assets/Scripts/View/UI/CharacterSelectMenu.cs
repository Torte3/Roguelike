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
    public class CharacterSelectMenu : MonoBehaviour, IMenu
    {
        public bool CanClose => true;
        [SerializeField] private CharacterDemoDisplay _characterDemoDisplay;
        private readonly ReactiveProperty<int> _selectedIndex = new(-1);
        public ReadOnlyReactiveProperty<int> SelectedIndex => _selectedIndex;
        private readonly AsyncReactiveProperty<int> _choicedIndex = new(-1);
        public IReadOnlyAsyncReactiveProperty<int> ChoicedIndex => _choicedIndex;
        [SerializeField] private RectTransform _content;
        [SerializeField] private ChoiceButton _choiceButtonPrefab;
        [SerializeField] private SEManager _seManager;
        [SerializeField] private TMP_Text _infoText;
        private readonly List<ChoiceButton> _buttons = new();

        public void SetChoices(List<(string name, string textureName, string info, bool usable)> characters)
        {
            foreach (var button in _buttons)
            {
                Destroy(button.gameObject);
            }

            _buttons.Clear();

            _selectedIndex.Value = 0;
            foreach (var (character, index) in characters.Index())
            {
                var button = Instantiate(_choiceButtonPrefab, _content);
                button.Construct(
                    character.name,
                    character.usable,
                    () =>
                    {
                        _seManager?.ChoiceCursorSE();
                        _selectedIndex.Value = index;
                        _characterDemoDisplay.SetTexture(character.textureName);
                        _characterDemoDisplay.SetColor(character.usable ? Color.white : Color.gray);
                        _infoText.text = character.info;
                    },
                    () =>
                    {
                        _seManager?.ChoiceConfirmSE();
                        _choicedIndex.Value = index;
                    });
                _buttons.Add(button);
            }

            for (var i = 0; i < _buttons.Count; i++)
            {
                var nav = new Navigation
                {
                    mode = Navigation.Mode.Explicit,
                    selectOnLeft = _buttons[(i - 1).WrapIndex(_buttons.Count)].GetComponent<Button>(),
                    selectOnRight = _buttons[(i + 1).WrapIndex(_buttons.Count)].GetComponent<Button>()
                };
                _buttons[i].GetComponent<Button>().navigation = nav;
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