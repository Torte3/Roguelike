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
                itemData => new ItemLibraryViewData(itemName, itemData.Icon, (int)itemData.Category, itemData.IsShiny, ItemDescriptionText.Of(new Item(itemData).DescribeIdentified())),
                directWeaponData => new ItemLibraryViewData(itemName, directWeaponData.Icon, (int)ItemCategory.Weapons, directWeaponData.IsShiny, ItemDescriptionText.Of(new DirectWeapon(directWeaponData).DescribeIdentified())),
                rangedWeaponData => new ItemLibraryViewData(itemName, rangedWeaponData.Icon, (int)ItemCategory.Weapons, rangedWeaponData.IsShiny, ItemDescriptionText.Of(new RangedWeapon(rangedWeaponData).DescribeIdentified())),
                artifactData => new ItemLibraryViewData(itemName, artifactData.Icon, (int)ItemCategory.Artifacts, artifactData.IsShiny, ItemDescriptionText.Of(new EquipmentItem(artifactData).DescribeIdentified()))
            );
            itemLibraryView.AddItem(itemName, itemViewData);
        }
    }
}