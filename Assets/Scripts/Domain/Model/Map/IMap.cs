#nullable enable
using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Map
{
    public interface IMap : IPassableChecker
    {
        public Id<IMap> Id { get; }
        public ItemDatabase ItemDatabase { get; }
        public ItemPlaceholders ItemPlaceholders { get; }
        public ItemMarketPriceTable MarketPriceTable { get; }
        public IShop? Shop { get; }
        public IPlayer Player { get; }

        public IObservableCollection<IEntity> Entities { get; }
        public IObservableCollection<ICharacter> Characters { get; }
        public IObservableCollection<IItemEntity> Items { get; }

        public HashSet<Vector2Int> GetAllPositions();

        public IEntity? GetEntityFastAt(Vector2Int position, EntityLayer layer);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position, params EntityLayer[] layers);
        public IEnumerable<IEntity> GetEntitiesFastAt(Vector2Int position);
        public IEntityEventEntity? GetEntityEventEntityFastAt(Vector2Int position, EntityLayer layer);
        public IEnumerable<IEntityEventEntity> GetEntityEventEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers);
        public IEnumerable<IEntityEventEntity> GetEntityEventEntitiesFastAt(Vector2Int position, params EntityLayer[] layers);
        public ICharacterEventEntity? GetCharacterEventEntityFastAt(Vector2Int position, EntityLayer layer);
        public IEnumerable<ICharacterEventEntity> GetCharacterEventEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers);
        public IEnumerable<ICharacterEventEntity> GetCharacterEventEntitiesFastAt(Vector2Int position, params EntityLayer[] layers);
        public IPlayerEventEntity? GetPlayerEventEntityFastAt(Vector2Int position, EntityLayer layer);
        public IEnumerable<IPlayerEventEntity> GetPlayerEventEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers);
        public IEnumerable<IPlayerEventEntity> GetPlayerEventEntitiesFastAt(Vector2Int position, params EntityLayer[] layers);
        public IScheduledEventEntity? GetScheduledEventEntityFastAt(Vector2Int position, EntityLayer layer);
        public IEnumerable<IScheduledEventEntity> GetScheduledEventEntitiesFastAt(Vector2Int position, IEnumerable<EntityLayer> layers);
        public IEnumerable<IScheduledEventEntity> GetScheduledEventEntitiesFastAt(Vector2Int position, params EntityLayer[] layers);
        public HashSet<Vector2Int> AllCharacterPositionsFast();
        public HashSet<Vector2Int> AllItemPositionsFast();

        public bool IsInside(Vector2Int position);
        public bool IsReachable(Vector2Int from, Vector2Int to, IHasBehavior actor);

        public IItem? GetItemByIdFromWorldOrInventory(Id<IItem> id);

        public UniTask ExecuteEntityTouchEventsAt(Vector2Int position, IEntity triggerEntity);
        public UniTask ExecuteCharacterTouchEventsAt(Vector2Int position, ICharacter character);

        public UniTask UpdateTurn(int turn);

        public void RemoveWalls(IEnumerable<Vector2Int> positions);

        public bool IsGrass(Vector2Int position);
        public void SetGrasses(IEnumerable<Vector2Int> positions, bool isGrass);
        public void SetIce(IEnumerable<Vector2Int> positions, bool isIce);

        public void RevealMimic(IEnumerable<Vector2Int> positions);
        public void AttackStatue(IEnumerable<Vector2Int> positions);

        public IItemEntity SpawnItem(IItem item, Vector2Int position);
        public bool SpawnRandomEnemy(Vector2Int position, bool? isSlept = null);
        public ICharacter? SpawnRandomEnemyIgnoreMimic(Vector2Int position, bool? isSlept = null);

        public void SpawnEnemy(EnemyData enemy, Vector2Int position, bool doActImmediately, IAffiliation? affiliation = null,
            bool? isSlept = null, bool? isShiny = null);

        public ICharacter SpawnEnemyIgnoreMimic(EnemyData enemy, Vector2Int position, bool doActImmediately, IAffiliation? affiliation = null,
            bool? isSlept = null, bool? isShiny = null);

        public void SpawnFire(IEnumerable<Vector2Int> positions);

        public UniTask<Vector2Int> ShowThrowAnimation(Sprite icon, Vector2Int position, Direction8 direction,
            int distance, bool isPiercing, params EntityLayer[] canHitLayer);

        public void SpawnEffect(IEnumerable<Vector2Int> area, Color color);

        public IItemEntity? TryPickUpAt(Vector2Int position, bool canPickUpShopItem);

        public Vector2Int FindBlankPositionFrom(Vector2Int position, Func<Vector2Int, bool> isBlankFunc);
        public Vector2Int GetThrowDestination(Vector2Int position, Direction8 direction, int distance, params EntityLayer[] canHitLayer);
        public IEnumerable<Vector2Int> GetThrowDestinationPiercing(Vector2Int position, Direction8 direction, int distance, params EntityLayer[] canHitLayer);

        public bool IsVisible(Vector2Int from, Vector2Int to, float radius);
        public HashSet<Vector2Int> GetVisibleArea(Vector2Int from, float radius);
        public HashSet<Vector2Int> GetFullVisibleArea();

        public HashSet<Vector2Int> ComputeCircle(Func<Vector2Int, bool> isTileBlocked, Vector2Int position,
            float radius);
    }
}