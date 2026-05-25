using Game;
using VContainer;
using View.UI;
using R3;

namespace Provider
{
    internal class MainMenuPresenter
    {
        [Inject]
        public MainMenuPresenter(MainMenu mainMenu, MenuController menuController, GameManager gameManager)
        {
            mainMenu.OnOpenSettingMenu.Subscribe(_ =>
            {
                menuController.PushSettingMenu();
            });
            mainMenu.OnOpenItemLibraryMenu.Subscribe(_ =>
            {
                menuController.PushItemLibraryMenu();
            });
            mainMenu.OnSaveAndReturnTitle.Subscribe(_ =>
            {
                menuController.PopMenu();
                gameManager.Save();
                gameManager.ReturnTitle();
            });
            mainMenu.OnSaveAndExit.Subscribe(_ =>
            {
                gameManager.Save();
                gameManager.Exit();
            });
        }
    }
}