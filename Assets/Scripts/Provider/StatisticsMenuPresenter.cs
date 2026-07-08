#nullable enable
using Game;
using R3;
using VContainer;
using View.UI;

namespace Provider
{
    internal class StatisticsMenuPresenter
    {
        [Inject]
        public StatisticsMenuPresenter(
            GameManager gameManager,
            StatisticsMenu statisticsMenu,
            MainMenu mainMenu,
            MenuController menuController)
        {
            mainMenu.OnOpenStatisticsMenu.Subscribe(_ =>
            {
                statisticsMenu.SetText(BuildStatisticsText(gameManager));
                menuController.PushStatisticsMenu();
            });

            gameManager.ActiveStatistics
                .Subscribe(_ => statisticsMenu.SetText(BuildStatisticsText(gameManager)));
            gameManager.GlobalStatistics.TotalTurns
                .Subscribe(_ => statisticsMenu.SetText(BuildStatisticsText(gameManager)));
        }

        private static string BuildStatisticsText(GameManager gameManager)
        {
            return StatisticsText.Of(gameManager.ActiveStatistics.CurrentValue?.Summarize(), gameManager.GlobalStatistics.Summarize());
        }
    }
}
