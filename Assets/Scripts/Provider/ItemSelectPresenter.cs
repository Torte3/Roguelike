#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
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
            ItemPreviewView itemPreviewView,
            InputReceiver receiver,
            IGameManager gameManager)
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
                var isItemPreviewVisible = false;

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
                        return;
                    }

                    previews = message.Previews.ToDictionary(preview => preview.Focus.ToInventoryViewIndex(), preview => preview);
                    defaultPreview = message.DefaultPreview;
                    previewTitle = message.PreviewTitle;
                    isItemPreviewVisible = true;
                    itemPreviewView.SetVisibility(true);
                    UpdatePreview(itemPreviewView, inventoryView.Focus.CurrentValue, previews, defaultPreview, map, previewTitle, isItemPreviewVisible);
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
                    if (isItemPreviewVisible)
                    {
                        itemPreviewView.SetVisibility(false);
                        isItemPreviewVisible = false;
                    }
                }).AddTo(disposables);

                inventoryView.Focus.Subscribe(focus =>
                {
                    gameManager.PlaySE(SE.ItemSelectCursor);

                    if (previews.Count == 0)
                        return;

                    UpdatePreview(itemPreviewView, focus, previews, defaultPreview, map, previewTitle, isItemPreviewVisible);
                }).AddTo(disposables);
            });
        }

        private static void UpdatePreview(
            ItemPreviewView itemPreviewView,
            InventoryViewIndex focus,
            IReadOnlyDictionary<InventoryViewIndex, ItemSelectPreview> previews,
            ItemSelectPreview? defaultPreview,
            IMap map,
            string previewTitle,
            bool isItemPreviewVisible)
        {
            if (!isItemPreviewVisible)
                return;

            if (!previews.TryGetValue(focus, out var preview))
            {
                if (defaultPreview == null)
                {
                    itemPreviewView.SetVisibility(false);
                    return;
                }
                preview = defaultPreview;
            }

            itemPreviewView.SetVisibility(true);
            itemPreviewView.SetPreview(
                previewTitle,
                ItemPreviewViewDataBuilder.Build(map, preview.Item),
                preview.Note);
        }
    }
}
