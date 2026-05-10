using Domain.Model;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events;
using Game;
using Provider.Input;
using Utilities;
using VContainer;
using VContainer.Unity;
using View;
using View.UI;
#if UNITY_EDITOR
using System.Text;
using Sirenix.OdinInspector;
using Unity.Logging;
using UnityEngine;
using UnityEditor;
#endif

namespace Provider
{
    internal class Container : LifetimeScope
    {
#if UNITY_EDITOR
        [ShowInInspector, ReadOnly, TextArea(20, 50)]
        private string _statisticsText = "";

        private GameManager? _gameManager;
#endif
        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<GameManager>(Lifetime.Singleton).AsSelf().As<IGameManager>();
            builder.Register<World>(Lifetime.Singleton);
            builder.Register<InputReceiver>(Lifetime.Singleton);
            builder.Register<GameInput>(Lifetime.Singleton);
            builder.Register<EffectViewSpawner>(Lifetime.Singleton);
            builder.Register<ChoiceReceiver>(Lifetime.Singleton);
            builder.Register<CharacterSelectReceiver>(Lifetime.Singleton);
            builder.Register<TextInputReceiver>(Lifetime.Singleton);
            builder.Register<CharacterControlInputReceiver>(Lifetime.Singleton);
            builder.Register<SynchronizedCharacterView>(Lifetime.Singleton);
            builder.Register<SynchronizedIconEntityView>(Lifetime.Singleton);
            builder.Register<SynchronizedThrowAnimationEntityView>(Lifetime.Singleton);
            builder.Register<SynchronizedFireEntityView>(Lifetime.Singleton);
            builder.RegisterComponentInHierarchy<DungeonInfoView>();
            builder.RegisterComponentInHierarchy<TilePalette>();
            builder.RegisterComponentInHierarchy<TileViewController>();
            builder.RegisterComponentInHierarchy<OverlayTileViewController>();
            builder.RegisterComponentInHierarchy<MinimapController>();
            builder.RegisterComponentInHierarchy<InventoryView>();
            builder.RegisterComponentInHierarchy<StatView>();
            builder.RegisterComponentInHierarchy<CameraFollowTarget>();
            builder.RegisterComponentInHierarchy<CameraFlameRect>();
            builder.RegisterComponentInHierarchy<MainMenu>();
            builder.RegisterComponentInHierarchy<SettingWindow>();
            builder.RegisterComponentInHierarchy<MenuController>();
            builder.RegisterComponentInHierarchy<ChoiceMenu>();
            builder.RegisterComponentInHierarchy<CharacterSelectMenu>();
            builder.RegisterComponentInHierarchy<LogView>();
            builder.RegisterComponentInHierarchy<ShopInfoView>();
            builder.RegisterComponentInHierarchy<ItemPreviewWindow>();
            builder.RegisterComponentInHierarchy<ItemSelectText>();
            builder.RegisterComponentInHierarchy<DamageTextSpawner>();
            builder.RegisterComponentInHierarchy<FlushController>();
            builder.RegisterComponentInHierarchy<BGMManager>();
            builder.RegisterComponentInHierarchy<SEManager>();
            builder.RegisterComponentInHierarchy<ItemLibraryView>();

            builder.RegisterPlainEntryPoint<InitPresenter>();
            builder.RegisterPlainEntryPoint<DungeonInfoPresenter>();
            builder.RegisterPlainEntryPoint<InputPresenter>();
            builder.RegisterPlainEntryPoint<TilemapPresenter>();
            builder.RegisterPlainEntryPoint<PlayerPresenter>();
            builder.RegisterPlainEntryPoint<PlayerInventoryPresenter>();
            builder.RegisterPlainEntryPoint<PlayerCameraPresenter>();
            builder.RegisterPlainEntryPoint<EffectPreviewPresenter>();
            builder.RegisterPlainEntryPoint<DamagePresenter>();
            builder.RegisterPlainEntryPoint<PickupEmphasisPresenter>();
            builder.RegisterPlainEntryPoint<SoundPresenter>();
            builder.RegisterPlainEntryPoint<GroupMarkerPresenter>();
            builder.RegisterPlainEntryPoint<KeyCharacterPresenter>();
            builder.RegisterPlainEntryPoint<MainMenuPresenter>();
            builder.RegisterPlainEntryPoint<SettingPresenter>();
            builder.RegisterPlainEntryPoint<LogPresenter>();
            builder.RegisterPlainEntryPoint<ShopInfoPresenter>();
            builder.RegisterPlainEntryPoint<StatisticsPresenter>();
            builder.RegisterPlainEntryPoint<ItemSelectPresenter>();
            builder.RegisterPlainEntryPoint<Presenter>();

            builder.RegisterPlainEntryPoint<DebugCommands>();
            builder.RegisterPlainEntryPoint<LogCommands>();
            builder.RegisterPlainEntryPoint<CharacterCommands>();
            builder.RegisterPlainEntryPoint<ItemCommands>();
            builder.RegisterPlainEntryPoint<SpawnCommands>();
            builder.RegisterPlainEntryPoint<MapCommands>();
        }
#if UNITY_EDITOR
        protected override void Awake()
        {
            base.Awake();
            _gameManager = Container.Resolve<GameManager>();
        }

        private void Update()
        {
            if (_gameManager == null) return;

            var sb = new StringBuilder();
            if (_gameManager.ActiveStatistics.CurrentValue != null)
            {
                sb.AppendLine(_gameManager.ActiveStatistics.CurrentValue.GetStatisticsText());
            }
            sb.AppendLine(_gameManager.GlobalStatistics.GetStatisticsText());
            _statisticsText = sb.ToString();
            EditorUtility.SetDirty(this);
        }
    }
#endif
}