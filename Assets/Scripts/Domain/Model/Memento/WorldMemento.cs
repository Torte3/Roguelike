#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model.Map;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;
using Utilities.Stats;

namespace Domain.Model.Memento
{
    public class WorldMemento
    {
        [field: SerializeField] public DungeonMemento Dungeon { get; private set; }
        [field: SerializeField] public PlayerMemento Player { get; private set; }
        [field: SerializeField] public List<CharacterMemento> PartyMembers { get; private set; }
        [field: SerializeField] public bool IsPlayerDead { get; private set; }
        [SerializeField] private List<string> _mapIds;
        public List<Id<IMap>> MapIds => _mapIds.Select(mapId => new Id<IMap>(mapId)).ToList();
        [SerializeField] private string _currentMapId;
        public Id<IMap> CurrentMapId => new(_currentMapId);
        [field: SerializeField] public ItemPlaceholdersMemento ItemPlaceholders { get; private set; }

        public WorldMemento(
            DungeonMemento dungeon,
            PlayerMemento player,
            List<CharacterMemento> partyMembers,
            bool isPlayerDead,
            List<Id<IMap>> mapIds,
            Id<IMap> currentMapId,
            ItemPlaceholdersMemento itemPlaceholders)
        {
            Dungeon = dungeon;
            Player = player;
            PartyMembers = partyMembers;
            IsPlayerDead = isPlayerDead;
            _mapIds = mapIds.Select(mapId => mapId.ToString()).ToList();
            _currentMapId = currentMapId.ToString();
            ItemPlaceholders = itemPlaceholders;
        }

        public WorldMemento CopyWith(
            DungeonMemento? dungeon = null,
            PlayerMemento? player = null,
            List<CharacterMemento>? partyMembers = null,
            bool? isPlayerDead = null,
            List<Id<IMap>>? mapIds = null,
            Id<IMap>? currentMapId = null,
            ItemPlaceholdersMemento? itemPlaceholders = null)
        {
            return new WorldMemento(
                dungeon ?? Dungeon,
                player ?? Player,
                partyMembers ?? PartyMembers,
                isPlayerDead ?? IsPlayerDead,
                mapIds ?? MapIds,
                currentMapId ?? CurrentMapId,
                itemPlaceholders ?? ItemPlaceholders);
        }

        public WorldMemento RevivePlayer()
        {
            return CopyWith(
                player: Player.CopyWith(
                    character: Player.Character.CopyWith(
                        status: Player.Character.Status.CopyWith(
                            stats: Player.Character.Status.Stats.CopyWith(
                                hp: new ResourceData(Player.Character.Status.Stats.Hp.Max, new Stat(Player.Character.Status.Stats.Hp.Max).CurrentValue)
                            )
                        ),
                        entity: Player.Character.Entity.CopyWith(
                            destroyLog: Option<string>.None
                        )
                    )
                )
            );
        }
    }
}