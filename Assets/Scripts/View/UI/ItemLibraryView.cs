#nullable enable
using System.Collections.Generic;
using System.Linq;
using R3;
using Sirenix.Utilities;
using TMPro;
using Unity.Logging;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace View.UI
{
    public record ItemLibraryViewData(string Name, Sprite Icon, int Category, bool IsShiny, string Info);
    public class ItemLibraryView : MonoBehaviour, IMenu
    {
        public bool CanClose => true;
        [SerializeField] private GameObject _content;
        [SerializeField] private InventoryItemView _itemViewPrefab;
        [SerializeField] private TMP_Text _infoText;
        private readonly List<ItemLibraryViewData> _items = new();
        private readonly List<InventoryItemView> _itemViews = new();

        public void AddItem(string name, ItemLibraryViewData itemViewData)
        {
            var index = _items.FindIndex(item => item.Name == name);
            if (index != -1)
                _items[index] = itemViewData;
            else
                _items.Add(itemViewData);
            GenerateViews();
        }

        private void GenerateViews()
        {
            Log.Debug($"GenerateViews: {_items.Count}");
            foreach (var view in _itemViews.WhereNotNull())
            {
                Destroy(view.gameObject);
            }
            _itemViews.Clear();
            var sortedItems = _items.OrderBy(item => item.Category).ThenBy(item => item.Name).ToList();
            foreach (var libraryItem in sortedItems)
            {
                var view = Instantiate(_itemViewPrefab, _content.transform);
                var itemViewData = new ItemViewData("", true, libraryItem.Icon, false, null, false, false, libraryItem.IsShiny, true, true, libraryItem.Info);
                view.Set(itemViewData);
                _itemViews.Add(view);
            }

            _itemViews.ForEach((view, index) => view.OnSelected.Subscribe(_ =>
            {
                _infoText.text = sortedItems[index].Info;
            }).AddTo(view));

            for (var i = 0; i < _itemViews.Count; i++)
                SetNavigation(i);
        }

        public void SetNavigation(int index)
        {
            var nav = new Navigation
            {
                mode = Navigation.Mode.Explicit,
                selectOnUp = _itemViews[(index - 10).WrapIndex(_itemViews.Count)].GetComponent<Selectable>(),
                selectOnDown = _itemViews[(index + 10).WrapIndex(_itemViews.Count)].GetComponent<Selectable>(),
                selectOnLeft = _itemViews[(index - 1).WrapIndex(_itemViews.Count)].GetComponent<Selectable>(),
                selectOnRight = _itemViews[(index + 1).WrapIndex(_itemViews.Count)].GetComponent<Selectable>()
            };
            _itemViews[index].GetComponent<Selectable>().navigation = nav;
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