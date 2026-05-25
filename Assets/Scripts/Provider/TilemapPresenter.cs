#nullable enable
using System;
using System.Linq;
using Domain.Model.Map;
using Game;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class TilemapPresenter
    {
        private readonly CompositeDisposable _disposables = new();

        [Inject]
        public TilemapPresenter(TileViewController tileView, OverlayTileViewController overlayTileView,
            MinimapController minimapController, TilePalette tilePalette, World world)
        {
            world.OnActiveMapChanged.Subscribe(mapChanged =>
                {
                    _disposables.Clear();
                    var map = mapChanged.Map;
                    tileView.Clear();
                    overlayTileView.Clear();
                    minimapController.Clear();

                    foreach (var (position, tileData) in map.TilemapViewer.GetAllTiles())
                    {
                        SetTile(tileView, minimapController, tilePalette, tileData, position, map.ShopRect);
                        if (map.TilemapViewer.GetAllGrasses().Contains(position))
                            overlayTileView.SetGrass(position, ToTileSet(tileData.MapType));
                        if (map.TilemapViewer.GetAllIces().Contains(position))
                            overlayTileView.SetIce(position);
                        SetVisibility(tileView, overlayTileView, minimapController, position,
                            GetTileVisibility(map, position));
                    }

                    map.TilemapViewer.OnTilesChanged.Subscribe(context =>
                    {
                        foreach (var (position, tile) in context)
                        {
                            SetTile(tileView, minimapController, tilePalette, tile, position, map.ShopRect);
                        }
                    }).AddTo(_disposables);

                    map.TilemapViewer.OnTilesLoaded.Subscribe(context =>
                    {
                        foreach (var (position, tile) in context)
                        {
                            SetTile(tileView, minimapController, tilePalette, tile, position, map.ShopRect);
                            SetVisibility(tileView, overlayTileView, minimapController, position,
                                GetTileVisibility(map, position));
                        }
                    }).AddTo(_disposables);

                    map.TilemapViewer.OnOverlayTilesChanged.Subscribe(context =>
                    {
                        foreach (var (position, category) in context)
                        {
                            switch (category)
                            {
                                case OverlayTileCategory.Grass:
                                    overlayTileView.SetGrass(
                                        position,
                                        map.TilemapViewer.GetTile(position)
                                            .Map(tile => ToTileSet(tile.MapType))
                                            .UnwrapOr(() => ToTileSet(map.Type)),
                                        GetTileVisibility(map, position));
                                    break;
                                case OverlayTileCategory.FloatingIce:
                                    overlayTileView.SetIce(position, GetTileVisibility(map, position));
                                    break;
                                case null:
                                    overlayTileView.RemoveTile(position);
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException(nameof(category), category, null);
                            }
                        }
                    }).AddTo(_disposables);
                    // HACK: The following subscription might conflict with the one below if their handling logic diverges in the future.
                    map.TilemapViewer.OnTilesKnownChanged.Subscribe(context =>
                    {
                        foreach (var (position, isKnown) in context)
                        {
                            if (isKnown)
                            {
                                SetVisibility(tileView, overlayTileView, minimapController, position,
                                    TileVisibility.Visible);
                            }
                            else
                            {
                                SetVisibility(tileView, overlayTileView, minimapController, position,
                                    TileVisibility.Transparent);
                            }
                        }
                    }).AddTo(_disposables);
                    // HACK: Here.
                    var previousVisibleArea = map.Player.Character.VisionRange.VisibleArea;
                    map.Player.Character.VisionRange.OnVisibleAreaChanged
                        .Select(x => map.Player.Character.VisionRange.VisibleArea)
                        .Subscribe(visibleAreaChanged =>
                        {
                            var areaEntered = visibleAreaChanged.Except(previousVisibleArea);
                            var areaExited = previousVisibleArea.Except(visibleAreaChanged);
                            previousVisibleArea = visibleAreaChanged;
                            foreach (var position in areaEntered)
                            {
                                SetVisibility(tileView, overlayTileView, minimapController, position,
                                    TileVisibility.Visible);
                            }

                            foreach (var position in areaExited)
                            {
                                SetVisibility(tileView, overlayTileView, minimapController, position,
                                    TileVisibility.Translucent);
                            }
                        }).AddTo(_disposables);
                },
                _ => _disposables.Clear());
        }

        public void Dispose()
        {
            _disposables.Dispose();
        }

        public TileVisibility GetTileVisibility(MapManager map, Vector2Int position)
        {
            if (map.TilemapViewer.GetTile(position).MapOr(false, tile => tile.IsKnown))
            {
                if (map.Player.Character.VisionRange.IsVisible(position))
                {
                    return TileVisibility.Visible;
                }

                return TileVisibility.Translucent;
            }

            return TileVisibility.Transparent;
        }

        public TileSet ToTileSet(MapType mapType)
        {
            return mapType switch
            {
                MapType.WorldMap => TileSet.WorldMap,
                MapType.Cave => TileSet.Cave,
                MapType.Forest => TileSet.Forest,
                MapType.Snow => TileSet.Snow,
                MapType.Volcano => TileSet.Volcano,
                MapType.Desert => TileSet.Desert,
                MapType.Dungeon => TileSet.Dungeon,
                MapType.Void => TileSet.Void,
                _ => throw new ArgumentOutOfRangeException(nameof(mapType), mapType, null)
            };
        }

        public void SetTile(TileViewController tileView, MinimapController minimapController, TilePalette tilePalette, Domain.Model.Map.TileData tileData,
            Vector2Int position, RectInt? shop, TileVisibility? visibility = null)
        {
            if (tileData.MapType == MapType.WorldMap)
            {
                var (tile, underTile) = tilePalette.GetTile(ToTileSet(tileData.MapType), tileData.Index);
                tileView.SetTile(position, tile, visibility, underTile);
            }
            else
            {
                var index = tileData.Category() switch
                {
                    TileCategory.Floor => shop.HasValue && shop.Value.Contains(position) ? 1 : 0,
                    TileCategory.Water => 2,
                    TileCategory.Wall => 3,
                    TileCategory.UnbreakableWall => 3,
                    _ => throw new ArgumentOutOfRangeException(nameof(TileCategory), tileData.Category(), null)
                };
                var (tile, underTile) = tilePalette.GetTile(ToTileSet(tileData.MapType), index);
                tileView.SetTile(position, tile, visibility, underTile);
            }
            switch (tileData.Category())
            {
                case TileCategory.Floor:
                    minimapController.SetFloor(position, visibility);
                    break;
                case TileCategory.Water:
                    minimapController.SetWater(position, visibility);
                    break;
                case TileCategory.Wall:
                    minimapController.SetWall(position, visibility);
                    break;
                case TileCategory.UnbreakableWall:
                    minimapController.SetUnbreakableWall(position, visibility);
                    break;
            }
        }

        public void SetVisibility(TileViewController tileView, OverlayTileViewController overlayTileView,
            MinimapController minimapController, Vector2Int position,
            TileVisibility visibility)
        {
            tileView.SetTileVisibility(position, visibility);
            overlayTileView.SetTileVisibility(position, visibility);
            minimapController.SetTileVisibility(position, visibility);
        }
    }
}