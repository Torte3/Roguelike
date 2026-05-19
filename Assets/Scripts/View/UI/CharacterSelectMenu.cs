#nullable enable
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model;
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;
using VContainer;

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
        private readonly List<ChoiceButton> _buttons = new();
        [SerializeField] private TMP_Text _infoText;
        private IGameManager _gameManager;

        [Inject]
        public void Construct(IGameManager gameManager)
        {
            _gameManager = gameManager;
        }

        public void SetChoices(List<(string name, string textureName, string info, bool usable)> characters)
        {
            foreach (var button in _buttons)
            {
                Destroy(button.gameObject);
            }

            _buttons.Clear();

            _selectedIndex.Value = 0;
            foreach (var (choice, index) in characters.Index())
            {
                var button = Instantiate(_choiceButtonPrefab, _content);
                button.Construct(
                    characters[index].name,
                    characters[index].usable,
                    () =>
                    {
                        _gameManager.PlaySE(SE.ChoiceCursor);
                        _selectedIndex.Value = index;
                        _characterDemoDisplay.SetTexture(characters[index].textureName);
                        if (characters[index].usable)
                            _characterDemoDisplay.SetColor(Color.white);
                        else
                            _characterDemoDisplay.SetColor(Color.gray);
                        _infoText.text = characters[index].info;
                    },
                    () =>
                    {
                        _gameManager.PlaySE(SE.ChoiceConfirm);
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