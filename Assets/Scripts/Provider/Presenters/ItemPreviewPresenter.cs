#nullable enable
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Service.Events.Receivers;
using Provider.Input;
using Provider.Texts;
using R3;
using VContainer;
using View.UI;
using View.UI.ItemPreview;
using View.UI.Menus;

namespace Provider.Presenters
{
    public class ItemPreviewPresenter
    {
        [Inject]
        public ItemPreviewPresenter(
            ChoiceReceiver choiceReceiver,
            MenuController menuController,
            ChoiceMenu choiceMenu,
            ItemPreviewView itemPreviewView)
        {
            choiceReceiver.OnShownChoiceWithItemPreview.Subscribe(async message =>
            {
                var choices = message.Items
                    .Select(item => (
                        Choice: new ItemNameText(item.NameIn(message.Map)).Text,
                        Preview: new ItemPreview(message.Map, item, true).ViewData))
                    .ToArray();

                itemPreviewView.SetVisibility(true);
                itemPreviewView.SetPreview(choices[0].Choice, choices[0].Preview);

                var disposable = choiceMenu.SelectedIndex.Subscribe(index =>
                {
                    if (index >= 0 && index < choices.Length)
                        itemPreviewView.SetPreview(choices[index].Choice, choices[index].Preview);
                });

                try
                {
                    var cancelIndex = message.CancelChoiceIndex;
                    var index = cancelIndex.HasValue
                        ? await menuController.GetChoice(new ToneText(message.Message).Text, cancelIndex.Value, choices.Select(x => x.Choice).ToArray())
                        : await menuController.GetChoice(new ToneText(message.Message).Text, choices.Select(x => x.Choice).ToArray());
                    choiceReceiver.SetChoicedIndex(index);
                }
                finally
                {
                    disposable.Dispose();
                    itemPreviewView.SetVisibility(false);
                }
            });
        }
    }
}
