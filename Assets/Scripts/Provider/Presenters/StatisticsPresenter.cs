#nullable enable
using Domain.Model.Dungeon;
using Domain.Model.Item.Data;
using Domain.Service.Items;
using Game;
using Provider.Texts;
using R3;
using Utilities;
using VContainer;
using View.UI.Menus;

namespace Provider.Presenters
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
                itemData => new ItemLibraryViewData(itemName, itemData.Icon, (int)itemData.Category, itemData.IsShiny, new ItemDescriptionText(new Item(itemData).DescribeIdentified()).Text),
                directWeaponData => new ItemLibraryViewData(itemName, directWeaponData.Icon, (int)ItemCategory.Weapons, directWeaponData.IsShiny, new ItemDescriptionText(new DirectWeapon(directWeaponData).DescribeIdentified()).Text),
                rangedWeaponData => new ItemLibraryViewData(itemName, rangedWeaponData.Icon, (int)ItemCategory.Weapons, rangedWeaponData.IsShiny, new ItemDescriptionText(new RangedWeapon(rangedWeaponData).DescribeIdentified()).Text),
                artifactData => new ItemLibraryViewData(itemName, artifactData.Icon, (int)ItemCategory.Artifacts, artifactData.IsShiny, new ItemDescriptionText(new EquipmentItem(artifactData).DescribeIdentified()).Text)
            );
            itemLibraryView.AddItem(itemName, itemViewData);
        }
    }
}