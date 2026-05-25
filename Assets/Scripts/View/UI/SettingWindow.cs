using System.Collections.Generic;
using R3;
using UnityEngine;

namespace View.UI
{
    public class SettingWindow : MonoBehaviour, IMenu
    {
        public bool CanClose => true;
        [SerializeField] private Transform _content;
        [SerializeField] private SliderOption _sliderItem;
        [SerializeField] private LabeledSliderOption _labeledSliderItem;
        [SerializeField] private CheckBoxOption _checkBoxItem;

        private ISettingOption firstOption;
        private ISettingOption lastOption;

        public void AddIntOption(string itemName, int min, int max, ReactiveProperty<int> value, ReadOnlyReactiveProperty<bool> isEnabled)
        {
            var option = Instantiate(_sliderItem, _content);
            option.SetData(itemName, min, max, value, isEnabled);
            UpdateNavigation(option);
        }

        public void AddLabeledIntOption(string itemName, IReadOnlyList<(int Value, string Label)> options, ReactiveProperty<int> index, ReadOnlyReactiveProperty<bool> isEnabled)
        {
            var option = Instantiate(_labeledSliderItem, _content);
            option.SetData(itemName, options, index, isEnabled);
            UpdateNavigation(option);
        }

        public void AddBoolOption(string itemName, ReactiveProperty<bool> value, ReadOnlyReactiveProperty<bool> isEnabled)
        {
            var option = Instantiate(_checkBoxItem, _content);
            option.SetData(itemName, value, isEnabled);
            UpdateNavigation(option);
        }

        public void Clear()
        {
            foreach (Transform child in _content)
                Destroy(child.gameObject);
            firstOption = null;
            lastOption = null;
        }

        private void UpdateNavigation(ISettingOption newSelectable)
        {
            if (firstOption == null)
            {
                firstOption = newSelectable;
                lastOption = newSelectable;
            }
            else
            {
                var lastNav = lastOption.Selectable.navigation;
                lastNav.selectOnDown = newSelectable.Selectable;
                lastOption.Selectable.navigation = lastNav;

                var firstNav = firstOption.Selectable.navigation;
                firstNav.selectOnUp = newSelectable.Selectable;
                firstOption.Selectable.navigation = firstNav;

                var newNav = newSelectable.Selectable.navigation;
                newNav.selectOnUp = lastOption.Selectable;
                newNav.selectOnDown = firstOption.Selectable;
                newSelectable.Selectable.navigation = newNav;

                lastOption = newSelectable;
            }
        }

        public void Show()
        {
            gameObject.SetActive(true);
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