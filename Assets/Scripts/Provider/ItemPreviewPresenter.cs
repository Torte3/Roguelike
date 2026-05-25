#nullable enable
using System.Linq;
using Cysharp.Threading.Tasks;
using Domain.Service.Events;
using Provider.Input;
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
                var choices = message.Choices
                    .Select(c => (c.Choice, Preview: ItemPreviewViewDataBuilder.Build(message.Map, c.Item, assumeIdentified: true)))
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
                        ? await menuController.GetChoice(message.Text, cancelIndex, choices.Select(x => x.Choice).ToArray())
                        : await menuController.GetChoice(message.Text, choices.Select(x => x.Choice).ToArray());
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
