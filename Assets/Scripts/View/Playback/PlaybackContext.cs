#nullable enable
using Utilities;
using VContainer;
using View.UI;

namespace View.Playback
{
    public sealed class PlaybackContext
    {
        [Inject]
        public PlaybackContext(EntityBoard entities, ProjectileLayer projectiles, ShownSight sight, TileBoard tiles,
            EffectViewSpawner effects, DamageTextSpawner damageTexts, FlushController flush, LogView log,
            StatView status, DungeonInfoView dungeonInfo, SEManager sounds, BGMManager music,
            CameraFollowTarget camera, CameraFlameRect cameraRect, MenuController menus, InventoryView inventory, ShopInfoView shopInfo)
        {
            Entities = entities;
            Projectiles = projectiles;
            Sight = sight;
            Tiles = tiles;
            Effects = effects;
            DamageTexts = damageTexts;
            Flush = flush;
            Log = log;
            Status = status;
            DungeonInfo = dungeonInfo;
            Sounds = sounds;
            Music = music;
            Camera = camera;
            CameraRect = cameraRect;
            Menus = menus;
            Inventory = inventory;
            inventory.Initialize();
            ShopInfo = shopInfo;
        }

        internal EntityBoard Entities { get; }
        internal ProjectileLayer Projectiles { get; }
        internal ShownSight Sight { get; }
        internal TileBoard Tiles { get; }
        internal EffectViewSpawner Effects { get; }
        internal DamageTextSpawner DamageTexts { get; }
        internal FlushController Flush { get; }
        internal LogView Log { get; }
        internal StatView Status { get; }
        internal DungeonInfoView DungeonInfo { get; }
        internal SEManager Sounds { get; }
        internal BGMManager Music { get; }
        internal CameraFollowTarget Camera { get; }
        internal CameraFlameRect CameraRect { get; }
        internal MenuController Menus { get; }
        internal InventoryView Inventory { get; }
        internal ShopInfoView ShopInfo { get; }
        internal PlaybackQueue Queue { get; set; } = null!;
    }
}
