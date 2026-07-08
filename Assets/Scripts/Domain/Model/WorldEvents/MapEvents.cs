#nullable enable
using System.Collections.Generic;
using Domain.Model.Effect.Skill;
using Domain.Model.Entity;
using Domain.Model.Map;
using UnityEngine;
using Utilities;

namespace Domain.Model.WorldEvents
{
    public record TileState(Vector2Int Position, MapType MapType, TileCategory Category, bool IsKnown);

    public record MapEntered(
        Id<IMap> Map,
        string Name,
        int Depth,
        MapType Type,
        RectInt Bounds,
        RectInt? ShopRect,
        ShopRelation Shop,
        Id<IEntity> Player,
        Vector2Int PlayerPosition,
        int Money,
        IReadOnlyList<TileState> Tiles,
        IReadOnlyList<(Vector2Int Position, OverlayTileCategory Category)> OverlayTiles,
        IReadOnlyCollection<Vector2Int> VisibleArea,
        bool IsNewWorld,
        InventoryContents Inventory,
        IReadOnlySkill BareHand) : AlwaysVisibleEvent;

    public record TilesChanged(IReadOnlyList<TileState> Tiles) : AlwaysVisibleEvent;

    public record ReturnedToTitle() : AlwaysVisibleEvent;

    public record OverlayTilesChanged(IReadOnlyList<(Vector2Int Position, OverlayTileCategory? Category)> Tiles)
        : AlwaysVisibleEvent;

    public record TilesKnown(IReadOnlyList<Vector2Int> Positions) : AlwaysVisibleEvent;
    public record TilesForgotten(IReadOnlyList<Vector2Int> Positions) : AlwaysVisibleEvent;
    public record SightChanged(IReadOnlyCollection<Vector2Int> VisibleArea) : AlwaysVisibleEvent;
    public record AreaEffectApplied(bool IsVisible, IReadOnlyList<Vector2Int> Area, Color Color, IReadOnlyList<EffectHit> Hits)
        : WorldEvent(IsVisible);
    public record GrassTrampled : AlwaysVisibleEvent;
    public record TurnPassed(int TurnInLevel) : AlwaysVisibleEvent;
}
