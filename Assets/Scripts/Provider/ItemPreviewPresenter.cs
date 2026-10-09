#nullable enable
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Model.Item;
using Domain.Service.Events;
using Provider.Input;
using Provider.Texts;
using R3;
using VContainer;
using View.UI;

namespace Provider
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
                        Choice: ItemNameText.Of(item.NameIn(message.Map)),
                        Preview: ItemPreviewViewDataBuilder.Build(message.Map, item, assumeIdentified: true)))
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
                    var index = message.CancelChoiceIndex is { } cancelIndex
                        ? await menuController.GetChoice(ToneText.Of(message.Message), cancelIndex, choices.Select(x => x.Choice).ToArray())
                        : await menuController.GetChoice(ToneText.Of(message.Message), choices.Select(x => x.Choice).ToArray());
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
