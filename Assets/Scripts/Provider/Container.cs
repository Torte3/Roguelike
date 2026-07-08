using System.Linq;
using Configuration;
using Domain.Model;
using Domain.Service.Characters.Behavior;
using Domain.Service.Events.Receivers;
using Game;
using Provider.Command;
using Provider.Input;
using Provider.Presentations;
using Provider.Presentations.Combat.SkillSources;
using Provider.Presentations.Inventory;
using Provider.Presentations.Tile;
using Provider.Presentations.Translation;
using Provider.Presenters;
using Provider.Texts;
using Provider.Texts.Labels;
using Utilities;
using VContainer;
using VContainer.Unity;
using View.Audio;
using View.Effects;
using View.Map;
using View.Playback;
using View.Playback.Drawing;
using View.UI;
using View.UI.HUD;
using View.UI.Inventory;
using View.UI.ItemPreview;
using View.UI.Menus;
using View.UI.Options;
#if UNITY_EDITOR
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
            builder.Register<World>(Lifetime.Singleton).AsSelf().As<IReadOnlyWorld>();
            builder.Register<InputReceiver>(Lifetime.Singleton).AsSelf().As<IDashInput>();
            builder.RegisterInstance(Settings.GlobalSettings);
            builder.Register<GameInput>(Lifetime.Singleton);
            builder.Register<EffectViewSpawner>(Lifetime.Singleton);
            builder.Register<DamageTextSpawner>(Lifetime.Singleton);
            builder.Register<ChoiceReceiver>(Lifetime.Singleton);
            builder.Register<CharacterSelectReceiver>(Lifetime.Singleton);
            builder.Register<TextInputReceiver>(Lifetime.Singleton);
            builder.Register<CharacterControlInputReceiver>(Lifetime.Singleton);
            builder.Register<TutorialReceiver>(Lifetime.Singleton);
            builder.Register<EntityBoard>(Lifetime.Singleton);
            builder.Register<ProjectileLayer>(Lifetime.Singleton);
            builder.Register<TileBoard>(Lifetime.Singleton);
            builder.Register<PlaybackContext>(Lifetime.Singleton);
            builder.Register<PlaybackBacklog>(Lifetime.Singleton);
            builder.Register<PlaybackClock>(Lifetime.Singleton);
            builder.RegisterEntryPoint<PlaybackQueue>().AsSelf();
            builder.Register<InventoryIcons>(Lifetime.Singleton);
            builder.Register<InventoryRows>(Lifetime.Singleton);
            builder.Register<ShopPrices>(Lifetime.Singleton);
            builder.Register<HealthCommands>(Lifetime.Singleton);
            builder.Register<BGMSelector>(Lifetime.Singleton);
            builder.Register<TileKindClassifier>(Lifetime.Singleton);
            builder.Register<PlayerSight>(Lifetime.Singleton);
            builder.Register<PlayerEntity>(Lifetime.Singleton);
            builder.Register<AffiliationMarkers>(Lifetime.Singleton);
            builder.Register<TileVisibilities>(Lifetime.Singleton);
            builder.Register<ChargePreviews>(Lifetime.Singleton);
            builder.Register<ChestKeyHolders>(Lifetime.Singleton);
            builder.Register<ProjectileSight>(Lifetime.Singleton);
            builder.Register<SkillSourcePresentations>(Lifetime.Singleton);
            builder.Register<WorldEventPresentations>(Lifetime.Singleton);
            foreach (var type in typeof(IWorldEventPresentation).Assembly.GetTypes()
                         .Where(type => typeof(IWorldEventPresentation).IsAssignableFrom(type) && !type.IsAbstract))
            {
                builder.Register(type, Lifetime.Singleton).As<IWorldEventPresentation>();
            }
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
            builder.RegisterComponentInHierarchy<StatisticsMenu>();
            builder.RegisterComponentInHierarchy<MenuController>();
            builder.RegisterComponentInHierarchy<ChoiceMenu>();
            builder.RegisterComponentInHierarchy<CharacterSelectMenu>();
            builder.RegisterComponentInHierarchy<LogView>();
            builder.RegisterComponentInHierarchy<ShopInfoView>();
            builder.RegisterComponentInHierarchy<ItemPreviewView>();
            builder.RegisterComponentInHierarchy<ItemSelectText>();
            builder.RegisterComponentInHierarchy<TextSpawner>();
            builder.RegisterComponentInHierarchy<FlushController>();
            builder.RegisterComponentInHierarchy<BGMManager>();
            builder.RegisterComponentInHierarchy<SEManager>();
            builder.RegisterComponentInHierarchy<ItemLibraryView>();
            builder.RegisterComponentInHierarchy<KeyHintView>();
            builder.RegisterComponentInHierarchy<TutorialWindow>();

            builder.RegisterPlainEntryPoint<InitPresenter>();
            builder.RegisterPlainEntryPoint<WorldEventTranslator>();
            builder.RegisterPlainEntryPoint<InputPresenter>();
            builder.RegisterPlainEntryPoint<EffectPreviewPresenter>();
            builder.RegisterPlainEntryPoint<MainMenuPresenter>();
            builder.RegisterPlainEntryPoint<SettingPresenter>();
            builder.RegisterPlainEntryPoint<KeyHintPresenter>();
            builder.RegisterPlainEntryPoint<TutorialPresenter>();
            builder.RegisterPlainEntryPoint<ItemPreviewPresenter>();
            builder.RegisterPlainEntryPoint<StatisticsPresenter>();
            builder.RegisterPlainEntryPoint<StatisticsMenuPresenter>();
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

            _statisticsText = new StatisticsReport(_gameManager.ActiveStatistics.CurrentValue?.Summarize(),
                _gameManager.GlobalStatistics.Summarize()).Text;
            EditorUtility.SetDirty(this);
        }
#endif
    }
}