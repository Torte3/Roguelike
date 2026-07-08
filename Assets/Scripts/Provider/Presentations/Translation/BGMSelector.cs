#nullable enable
using System;
using System.Collections.Generic;
using Domain.Model.WorldEvents;
using View.Playback;
using View.Playback.Commands;
using View.Playback.Values;

namespace Provider.Presentations.Translation
{
    public sealed class BGMSelector
    {
        private Guid? _map;

        internal IReadOnlyList<ViewCommand> MapEntered(Guid map, ShopRelation shop)
        {
            if (_map.HasValue && _map.Value == map)
                return Array.Empty<ViewCommand>();

            _map = map;
            return new[] { ShopRelationChanged(shop) };
        }

        internal ViewCommand ShopRelationChanged(ShopRelation shop) => new PlayBGM(TrackOf(shop));

        private static BGMTrack TrackOf(ShopRelation shop)
        {
            return shop switch
            {
                NoShop => BGMTrack.Normal,
                OutsideShop => BGMTrack.Normal,
                InsideShop => BGMTrack.Shop,
                ShopAfterTheft => BGMTrack.Stolen,
                _ => throw new ArgumentOutOfRangeException(nameof(shop), shop, null),
            };
        }

        internal ViewCommand MonsterHouseEntered() => new PlayBGM(BGMTrack.MonsterHouse);
    }
}
