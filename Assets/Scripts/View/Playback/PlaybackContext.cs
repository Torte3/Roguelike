#nullable enable
using Configuration;
using Utilities;
using VContainer;
using View.Audio;
using View.Effects;
using View.Playback.Drawing;
using View.UI;
using View.UI.HUD;
using View.UI.Inventory;

namespace View.Playback
{
    public sealed class PlaybackContext
    {
        [Inject]
        public PlaybackContext(EntityBoard entities, ProjectileLayer projectiles, TileBoard tiles,
            EffectViewSpawner effects, DamageTextSpawner damageTexts, FlushController flush, LogView log,
            StatView status, DungeonInfoView dungeonInfo, SEManager seManager, BGMManager bgmManager,
            CameraFollowTarget camera, CameraFlameRect cameraRect, MenuController menus, InventoryView inventory, ShopInfoView shopInfo,
            PlaybackClock clock, GlobalSettings settings)
        {
            Entities = entities;
            Projectiles = projectiles;
            Tiles = tiles;
            Effects = effects;
            DamageTexts = damageTexts;
            Flush = flush;
            Log = log;
            Status = status;
            DungeonInfo = dungeonInfo;
            SEManager = seManager;
            BGMManager = bgmManager;
            Camera = camera;
            CameraRect = cameraRect;
            Menus = menus;
            Inventory = inventory;
            ShopInfo = shopInfo;
            Clock = clock;
            Settings = settings;
        }

        internal EntityBoard Entities { get; }
        internal ProjectileLayer Projectiles { get; }
        internal TileBoard Tiles { get; }
        internal EffectViewSpawner Effects { get; }
        internal DamageTextSpawner DamageTexts { get; }
        internal FlushController Flush { get; }
        internal LogView Log { get; }
        internal StatView Status { get; }
        internal DungeonInfoView DungeonInfo { get; }
        internal SEManager SEManager { get; }
        internal BGMManager BGMManager { get; }
        internal CameraFollowTarget Camera { get; }
        internal CameraFlameRect CameraRect { get; }
        internal MenuController Menus { get; }
        internal InventoryView Inventory { get; }
        internal ShopInfoView ShopInfo { get; }
        internal PlaybackClock Clock { get; }
        internal GlobalSettings Settings { get; }
    }
}
