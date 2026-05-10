#nullable enable
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Service.Items;
using Game;
using R3;
using Utilities;
using VContainer;
using View.UI;

namespace Provider
{
    public class StatisticsPresenter
    {
        [Inject]
        public StatisticsPresenter(GameManager gameManager, ItemLibraryView itemLibraryView)
        {
            var _disposable = new SerialDisposable();

            gameManager.GlobalStatistics.KnownItemNames.SubscribeIncludingCurrentItems(collectionChanged =>
            {
                AddItemView(itemLibraryView, collectionChanged);
            });
        }

        private void AddItemView(ItemLibraryView itemLibraryView, string itemName)
        {
            var baseItemData = ScriptableObjectLoaderExtension.LoadItemData(itemName);
            var itemViewData = baseItemData.Match(
                itemData => new ItemLibraryViewData(itemName, itemData.Icon, (int)itemData.Category, itemData.IsShiny, new Item(itemData).FullInfo()),
                directWeaponData => new ItemLibraryViewData(itemName, directWeaponData.Icon, (int)ItemCategory.Weapons, directWeaponData.IsShiny, new DirectWeapon(directWeaponData).FullInfo()),
                rangedWeaponData => new ItemLibraryViewData(itemName, rangedWeaponData.Icon, (int)ItemCategory.Weapons, rangedWeaponData.IsShiny, new RangedWeapon(rangedWeaponData).FullInfo()),
                artifactData => new ItemLibraryViewData(itemName, artifactData.Icon, (int)ItemCategory.Artifacts, artifactData.IsShiny, new Artifact(artifactData).FullInfo())
            );
            itemLibraryView.AddItem(itemName, itemViewData);
        }
    }
}