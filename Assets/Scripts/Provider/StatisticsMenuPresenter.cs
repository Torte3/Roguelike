#nullable enable
using System.Text;
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
            var sb = new StringBuilder();
            if (gameManager.ActiveStatistics.CurrentValue != null)
                sb.AppendLine(gameManager.ActiveStatistics.CurrentValue.GetStatisticsText());
            sb.AppendLine(gameManager.GlobalStatistics.GetStatisticsText());
            return sb.ToString();
        }
    }
}
