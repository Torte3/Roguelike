#nullable enable
using System;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Memento
{
    [Serializable]
    public class MapMemento
    {
        [SerializeField] private string _id;
        public Id<IMap> Id => new(_id);
        [field: SerializeField] public TilemapMemento Tilemap { get; private set; }
        [field: SerializeField] public EntitiesMemento Entities { get; private set; }
        [field: SerializeField] public Option<RoomMemento> MonsterHouse { get; private set; }
        [field: SerializeField] public Option<ShopMemento> Shop { get; private set; }
        [field: SerializeField] public Vector2Int RandomBlankPosition { get; private set; }

        public MapMemento(
            Id<IMap> id,
            TilemapMemento tilemap,
            EntitiesMemento entities,
            Option<RoomMemento> monsterHouse,
            Option<ShopMemento> shop,
            Vector2Int randomBlankPosition)
        {
            _id = id.ToString();
            Tilemap = tilemap;
            Entities = entities;
            MonsterHouse = monsterHouse;
            Shop = shop;
            RandomBlankPosition = randomBlankPosition;
        }
    }
}