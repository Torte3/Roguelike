#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Game;
using Provider.Input;
using R3;
using VContainer;
using View.UI;

namespace Provider
{
    public class ItemSelectPresenter
    {
        [Inject]
        public ItemSelectPresenter(
            World world,
            ItemSelectText itemSelectText,
            InventoryView inventoryView,
            ItemPreviewWindow itemPreviewWindow,
            InputReceiver receiver)
        {
            var serialDisposable = new SerialDisposable();
            world.OnActiveMapChanged.Subscribe(mapChanged =>
            {
                var disposables = new CompositeDisposable();
                serialDisposable.Disposable = disposables;

                var map = mapChanged.Map;
                var player = mapChanged.Map.Player.Character;
                var previews = new Dictionary<InventoryViewIndex, ItemSelectPreview>();
                ItemSelectPreview? defaultPreview = null;
                var previewTitle = string.Empty;
                itemPreviewWindow.SetVisibility(false);

                player.OnStartItemSelect.Subscribe(message =>
                {
                    itemSelectText.Show(message.Text);
                    inventoryView.LockItems(message.DisabledItemIndexes.Select(index => index.ToInventoryViewIndex()).ToList());
                    inventoryView.SetCanSkip(true);
                    receiver.SwitchMenu();

                    if (message.Previews == null || message.Previews.Length == 0)
                    {
                        previews.Clear();
                        defaultPreview = null;
                        previewTitle = string.Empty;
                        itemPreviewWindow.SetVisibility(false);
                        return;
                    }

                    previews = message.Previews.ToDictionary(preview => preview.Focus.ToInventoryViewIndex(), preview => preview);
                    defaultPreview = message.DefaultPreview;
                    previewTitle = message.PreviewTitle;
                    UpdatePreview(itemPreviewWindow, inventoryView.Focus.CurrentValue, previews, defaultPreview, map, previewTitle);
                }).AddTo(disposables);

                player.OnSelectedItemSelect.Subscribe(_ =>
                {
                    itemSelectText.Hide();
                    inventoryView.UnlockAllItems();
                    inventoryView.SetCanSkip(false);
                    receiver.SwitchField();
                    previews.Clear();
                    defaultPreview = null;
                    previewTitle = string.Empty;
                    itemPreviewWindow.SetVisibility(false);
                }).AddTo(disposables);

                inventoryView.Focus.Subscribe(focus =>
                {
                    if (previews.Count == 0)
                    {
                        return;
                    }

                    UpdatePreview(itemPreviewWindow, focus, previews, defaultPreview, map, previewTitle);
                }).AddTo(disposables);
            });
        }

        private static void UpdatePreview(
            ItemPreviewWindow itemPreviewWindow,
            InventoryViewIndex focus,
            IReadOnlyDictionary<InventoryViewIndex, ItemSelectPreview> previews,
            ItemSelectPreview? defaultPreview,
            IMap map,
            string previewTitle)
        {
            if (!previews.TryGetValue(focus, out var preview))
            {
                if (defaultPreview == null)
                {
                    itemPreviewWindow.SetVisibility(false);
                    return;
                }
                preview = defaultPreview;
            }

            itemPreviewWindow.SetVisibility(true);
            itemPreviewWindow.SetPreview(previewTitle, BuildItemViewData(map, preview.Item), preview.Note);
        }

        private static ItemViewData BuildItemViewData(IMap map, IItem item)
        {
            var baseName = item.GetName(map.Player, map.ItemPlaceholders);
            return new ItemViewData(
                baseName,
                item.CanActivateWhenUsed,
                item.Icon,
                canSelect: true,
                item.HasActivatableSkill ? item.RemainingUses.CurrentValue : null,
                item.IsCursed,
                item.IsShiny,
                map.Player.Character.IsKnownItem(item),
                item.IsCurseIdentified,
                item.Info(map.Player, map.ItemPlaceholders)
            );
        }
    }
}