using R3;
using UnityEngine;

namespace View.UI
{
    public class MainMenu : MonoBehaviour, IMenu
    {
        public bool CanClose => true;
        private readonly Subject<Unit> _onSaveAndReturnTitle = new();
        private readonly Subject<Unit> _onSaveAndExit = new();
        private readonly Subject<Unit> _onOpenSettingMenu = new();
        private readonly Subject<Unit> _onOpenItemLibraryMenu = new();
        private readonly Subject<Unit> _onOpenStatisticsMenu = new();
        public Observable<Unit> OnSaveAndReturnTitle => _onSaveAndReturnTitle;
        public Observable<Unit> OnSaveAndExit => _onSaveAndExit;
        public Observable<Unit> OnOpenSettingMenu => _onOpenSettingMenu;
        public Observable<Unit> OnOpenItemLibraryMenu => _onOpenItemLibraryMenu;
        public Observable<Unit> OnOpenStatisticsMenu => _onOpenStatisticsMenu;
        public void OpenSettingMenu()
        {
            _onOpenSettingMenu.OnNext(Unit.Default);
        }
        public void OpenItemLibraryMenu()
        {
            _onOpenItemLibraryMenu.OnNext(Unit.Default);
        }
        public void OpenStatisticsMenu()
        {
            _onOpenStatisticsMenu.OnNext(Unit.Default);
        }
        public void SaveAndReturnTitle()
        {
            _onSaveAndReturnTitle.OnNext(Unit.Default);
        }
        public void SaveAndExit()
        {
            _onSaveAndExit.OnNext(Unit.Default);
        }
        public void Show()
        {
            gameObject.SetActive(true);
        }

        public void Hide()
        {
            gameObject.SetActive(false);
        }

        public void Enable()
        {
        }

        public void Disable()
        {
        }
    }
}