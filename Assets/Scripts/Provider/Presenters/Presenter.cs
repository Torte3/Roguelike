#nullable enable
using Cysharp.Threading.Tasks;
using Game;
using R3;
using Unity.Logging;
using VContainer;
using View.Playback;
using View.UI;

namespace Provider.Presenters
{
    public class Presenter
    {

        [Inject]
        public Presenter(GameManager gameManager, MenuController menuController, PlaybackQueue playback)
        {
            gameManager.SetReadyWaiter(playback.WaitUntilIdle);
            gameManager.State.Subscribe(state =>
            {
                switch (state)
                {
                    case GameState.Title:
                        Log.Debug("[Game]Change to title scene.");
                        gameManager.Title().Forget();
                        menuController.TitleMenu();
                        break;
                    case GameState.Dungeon:
                        Log.Debug("[Game]Change to dungeon scene.");
                        menuController.DungeonMenu();
                        break;
                }
            });
        }
    }
}