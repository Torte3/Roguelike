#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model;
using ObservableCollections;
using R3;
using Unity.Logging;
using UnityEngine;
using UnityEngine.EventSystems;
using VContainer;

namespace View.UI
{
    public class MenuController : MonoBehaviour
    {
        [SerializeField] private TitleMenu _titleMenu;
        [SerializeField] private DungeonMenu _dungeonMenu;
        [SerializeField] private SettingWindow _settingMenu;
        [SerializeField] private StatisticsMenu _statisticsMenu;
        [SerializeField] private ItemLibraryView _itemLibraryMenu;
        [SerializeField] private InfoMenu _infoMenu;
        [SerializeField] private ChoiceMenu _choiceMenu;
        [SerializeField] private CharacterSelectMenu _characterSelectMenu;
        [SerializeField] private TextInputMenu _textInputMenu;
        [SerializeField] private MainMenu _mainMenu;
        private readonly ObservableStack<IMenu> _menuStack = new();
        private readonly Dictionary<IMenu, GameObject> _selectedObject = new();
        private ReactiveProperty<MenuType> _menuState = new(MenuType.Menu);
        public ReadOnlyReactiveProperty<MenuType> MenuState => _menuState;

        [Inject]
        public void Construct()
        {
            _menuStack.ObserveChanged().Subscribe(_ =>
            {
                var menuType = GetCurrentMenuType();
                if (!menuType.HasValue)
                    return;
                if (menuType == _menuState.CurrentValue)
                    return;
                _menuState.Value = menuType.Value;
            });
        }

        public void OpenMeinMenu()
        {
            if (!_menuStack.Contains(_mainMenu))
            {
                AddMenu(_mainMenu);
            }
        }

        public void CloseMenu()
        {
            if (_menuStack.Count > 0 && _menuStack.Peek().CanClose)
            {
                PopMenu();
            }
        }

        public void CloseAllMenus()
        {
            if (!_menuStack.Contains(_mainMenu))
                return;

            while (_menuStack.Count > 0 && _menuStack.Peek().CanClose)
            {
                PopMenu();
            }
        }

        private MenuType? GetCurrentMenuType()
        {
            if (_menuStack.Count == 0)
                return null;
            return _menuStack.Peek() == _dungeonMenu ? MenuType.Field : MenuType.Menu;
        }

        public async UniTask<int> GetChoiceWithInfo(
            string? text,
            params (string choice, string infoTitle, string info)[] choices)
        {
            AddMenu(_infoMenu);
            var disposable = _choiceMenu.SelectedIndex.Subscribe(index =>
            {
                _infoMenu.SetInfo(choices[index].infoTitle, choices[index].info);
            });
            var choiceIndex = await GetChoice(text, choices.Select(x => x.choice).ToArray());
            disposable.Dispose();
            PopMenu();
            return choiceIndex;
        }

        public async UniTask<int> GetChoiceWithInfo(
            string? text,
            int cancelChoiceIndex,
            params (string choice, string infoTitle, string info)[] choices)
        {
            AddMenu(_infoMenu);
            var disposable = _choiceMenu.SelectedIndex.Subscribe(index =>
            {
                _infoMenu.SetInfo(choices[index].infoTitle, choices[index].info);
            });
            var choiceIndex = await GetChoice(text, cancelChoiceIndex, choices.Select(x => x.choice).ToArray());
            disposable.Dispose();
            PopMenu();
            return choiceIndex;
        }

        public UniTask<int> GetChoice(string? text, params string[] choices) =>
            GetChoiceInternal(text, null, choices);

        public UniTask<int> GetChoice(string? text, int cancelChoiceIndex, params string[] choices) =>
            GetChoiceInternal(text, cancelChoiceIndex, choices);

        private async UniTask<int> GetChoiceInternal(string? text, int? cancelChoiceIndex, string[] choices)
        {
            _choiceMenu.SetCanCancel(cancelChoiceIndex.HasValue);
            _choiceMenu.SetChoices(text, choices);
            await UniTask.NextFrame();
            AddMenu(_choiceMenu);
            var choiceIndex = await WaitForChoice(
                _choiceMenu.ChoicedIndex,
                _choiceMenu,
                waitForClose: cancelChoiceIndex.HasValue,
                choiceIndexWhenClosed: cancelChoiceIndex);
            if (IsMenuOpen(_choiceMenu))
                PopMenu();
            return choiceIndex!.Value;
        }

        public async UniTask<int?> GetCharacter(List<(string name, string textureName, string info, bool usable)> characters)
        {
            _characterSelectMenu.SetChoices(characters);
            await UniTask.NextFrame();
            AddMenu(_characterSelectMenu);
            var choiceIndex = await WaitForChoice(
                _characterSelectMenu.ChoicedIndex,
                _characterSelectMenu,
                waitForClose: true);
            if (IsMenuOpen(_characterSelectMenu))
                PopMenu();
            return choiceIndex;
        }

        private async UniTask<int?> WaitForChoice(
            IReadOnlyAsyncReactiveProperty<int> choice,
            IMenu menu,
            bool waitForClose,
            int? choiceIndexWhenClosed = null)
        {
            if (!waitForClose)
                return await choice.WaitAsync();

            var (hasChoice, index) = await UniTask.WhenAny(
                choice.WaitAsync(),
                UniTask.WaitUntil(() => !IsMenuOpen(menu))
            );
            return hasChoice ? index : choiceIndexWhenClosed;
        }

        private bool IsMenuOpen(IMenu menu) =>
            _menuStack.Count > 0 && _menuStack.Peek() == menu;

        public async UniTask<string?> GetTextInput(bool canCancel = false)
        {
            _textInputMenu.SetCanCancel(canCancel);
            AddMenu(_textInputMenu);
            var text = canCancel
                ? await WaitTextOrClose(_textInputMenu)
                : await _textInputMenu.Text.WaitAsync();
            if (IsMenuOpen(_textInputMenu))
                PopMenu();
            return text;
        }

        private async UniTask<string?> WaitTextOrClose(TextInputMenu menu)
        {
            var (hasText, text) = await UniTask.WhenAny(
                menu.Text.WaitAsync(),
                UniTask.WaitUntil(() => !IsMenuOpen(menu))
            );
            return hasText ? text : null;
        }

        public void SwitchMenu(IMenu menu)
        {
            Log.Info($"[Menu]SwitchMenu: {menu} MenuStack Count: {_menuStack.Count}");
            if (_menuStack.Count > 0)
            {
                var previousMenu = _menuStack.Peek();
                _selectedObject[previousMenu] = EventSystem.current.currentSelectedGameObject;
                previousMenu.Hide();
                previousMenu.Disable();
            }

            if (_selectedObject.ContainsKey(menu))
                EventSystem.current.SetSelectedGameObject(_selectedObject[menu]);
            menu.Show();
            menu.Enable();
            _menuStack.Clear();
            _menuStack.Push(menu);
        }

        public void PushMenu(IMenu pushedMenu)
        {
            Log.Info($"[Menu]PushMenu: {pushedMenu} MenuStack Count: {_menuStack.Count}");
            if (_menuStack.Count > 0)
            {
                var previousMenu = _menuStack.Peek();
                _selectedObject[previousMenu] = EventSystem.current.currentSelectedGameObject;
                previousMenu.Hide();
                previousMenu.Disable();
            }

            EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(pushedMenu));
            pushedMenu.Show();
            pushedMenu.Enable();
            _menuStack.Push(pushedMenu);
        }

        public void AddMenu(IMenu addedMenu)
        {
            Log.Info($"[Menu]AddMenu: {addedMenu} MenuStack Count: {_menuStack.Count}");
            if (_menuStack.Count > 0)
            {
                var previousMenu = _menuStack.Peek();
                _selectedObject[previousMenu] = EventSystem.current.currentSelectedGameObject;
                previousMenu.Disable();
            }

            EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(addedMenu));
            addedMenu.Show();
            addedMenu.Enable();
            _menuStack.Push(addedMenu);
        }

        public void PopMenu()
        {
            var poppedMenu = _menuStack.Pop();
            Log.Info($"[Menu]PopMenu: {poppedMenu} MenuStack Count: {_menuStack.Count}");
            _selectedObject[poppedMenu] = EventSystem.current.currentSelectedGameObject;
            if (_menuStack.Count > 0)
            {
                var previousMenu = _menuStack.Peek();
                EventSystem.current.SetSelectedGameObject(_selectedObject.GetValueOrDefault(previousMenu));
                previousMenu.Show();
                previousMenu.Enable();
            }

            poppedMenu.Hide();
            poppedMenu.Disable();
        }

        public void TitleMenu()
        {
            SwitchMenu(_titleMenu);
        }

        public void TitleMenuWhenGameOver(int level, float score, string causeOfDeath)
        {
            _titleMenu.SetData(level, score, causeOfDeath);
            SwitchMenu(_titleMenu);
        }

        public void DungeonMenu()
        {
            SwitchMenu(_dungeonMenu);
        }

        public void PushSettingMenu()
        {
            PushMenu(_settingMenu);
        }

        public void PushItemLibraryMenu()
        {
            PushMenu(_itemLibraryMenu);
        }

        public void PushStatisticsMenu()
        {
            PushMenu(_statisticsMenu);
        }
    }
}