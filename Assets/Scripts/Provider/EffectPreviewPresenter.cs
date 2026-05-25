#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Item;
using Domain.Model.Setting;
using Domain.Service.Effect;
using Game;
using R3;
using UnityEngine;
using VContainer;
using View;
using View.UI;

namespace Provider
{
    public class EffectPreviewPresenter
    {
        [Inject]
        public EffectPreviewPresenter(GameManager gameManager, World world, EffectViewSpawner effectViewSpawner,
            InventoryView inventoryView)
        {
            var disposables = new CompositeDisposable();
            var previews = new List<GameObject>();
            world.OnActiveMapChanged.Subscribe(mapChanged =>
                {
                    var map = mapChanged.Map;
                    var player = map.Player.Character;
                    disposables.Add(map.OnEffectSpawned.Subscribe(effectSpawned =>
                        effectViewSpawner.Spawn(
                            effectSpawned.Area.Intersect(player.VisibleArea),
                            effectSpawned.Color,
                            Settings.GlobalSettings.EffectDisplayTime.CurrentValue
                        )
                    ));
                    disposables.Add(Observable.Merge(
                        inventoryView.Focus.AsUnitObservable(),
                        player.Inventory.OnItemRemoved.AsUnitObservable(),
                        player.Inventory.OnItemReplaced.AsUnitObservable(),
                        player.Direction.AsUnitObservable(),
                        gameManager.OnTurnChanged
                    ).Subscribe(_ =>
                    {
                        previews.ForEach(preview => Object.Destroy(preview));
                        previews.Clear();
                        if (player.IsDead)
                        {
                            return;
                        }

                        var focus = inventoryView.Focus.CurrentValue;
                        if (!focus.IsOnEmpty)
                        {
                            IItem? item = null;
                            if (focus.IsOnGroundItem)
                            {
                                item = map.Items.At(player.Entity.CurrentPosition).FirstOrDefault()?.Item;
                            }
                            else
                            {
                                item = player.Inventory.GetItem(focus.Index);
                            }

                            if (item != null && player.IsKnownItem(item))
                            {
                                if (item.SkillOnUse.HasValue &&
                                    item.SkillOnUse.Value.Skill is SpawnEffectSkill spawnEffectSkill)
                                {
                                    var area = spawnEffectSkill.GetArea(player,
                                        player.Entity.CurrentPosition,
                                        player.CurrentDirection, map, true);
                                    var color = spawnEffectSkill.Color;
                                    color.a = 0.25f;
                                    previews = effectViewSpawner.SpawnPreview(area, color);
                                }
                            }
                        }
                    }));
                },
                _ => disposables.Clear()
            );
        }
    }
}