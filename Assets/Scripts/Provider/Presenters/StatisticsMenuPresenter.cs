#nullable enable
using Game;
using Provider.Texts;
using R3;
using VContainer;
using View.UI;
using View.UI.Menus;

namespace Provider.Presenters
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
            return new StatisticsReport(gameManager.ActiveStatistics.CurrentValue?.Summarize(),
                gameManager.GlobalStatistics.Summarize()).Text;
        }
    }
}
