#nullable enable
using System.Collections.Generic;
using Domain.Model.Map;
using UnityEngine;

namespace Domain.Model.WorldEvents
{
    public record TileState(Vector2Int Position, MapType MapType, TileCategory Category, bool IsKnown);

    public record MapEntered(
        string Name,
        int Depth,
        MapType Type,
        RectInt Bounds,
        RectInt? ShopRect,
        bool IsInShop,
        bool IsShopStolen,
        Vector2Int PlayerPosition,
        int Money,
        IReadOnlyList<TileState> Tiles,
        IReadOnlyList<(Vector2Int Position, OverlayTileCategory Category)> OverlayTiles,
        IReadOnlyCollection<Vector2Int> VisibleArea,
        bool IsNewWorld,
        InventoryLook? Inventory,
        Underfoot? Underfoot,
        ShopLook? Shop) : AlwaysVisibleEvent, IInventoryEvent, IUnderfootEvent, IShopEvent;

    public record TilesChanged(IReadOnlyList<TileState> Tiles) : AlwaysVisibleEvent;

    public record OverlayTilesChanged(IReadOnlyList<(Vector2Int Position, OverlayTileCategory? Category)> Tiles)
        : AlwaysVisibleEvent;

    public record TilesKnownChanged(IReadOnlyList<Vector2Int> Positions, bool IsKnown) : AlwaysVisibleEvent;
    public record SightChanged(IReadOnlyCollection<Vector2Int> VisibleArea) : AlwaysVisibleEvent;
    public record AreaEffectApplied(bool IsVisible, IReadOnlyList<Vector2Int> Area, Color Color, IReadOnlyList<EffectHit> Hits)
        : WorldEvent(IsVisible);
    public record GrassTrampled : AlwaysVisibleEvent;
    public record TurnPassed(int TurnInLevel) : AlwaysVisibleEvent;
}
