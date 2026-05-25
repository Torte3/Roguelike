#nullable enable
using Domain.Model.Setting;
using Game;
using R3;
using UnityEngine;
using Utilities;
using VContainer;
using View;
using View.UI;

namespace Provider
{
    public class PlayerPresenter
    {
        [Inject]
        public PlayerPresenter(World world, SynchronizedCharacterView characters,
            StatView statView)
        {
            CompositeDisposable _disposables = new();
            world.OnActiveMapChanged.Subscribe(mapChanged =>
                {
                    var player = mapChanged.Map.Player;
                    if (player.Character.IsDead)
                    {
                        return;
                    }

                    var playerView = characters.Get(player.Character);

                    var arrowPrefab = ObjectLoader.LoadPrefab("Arrow");
                    var arrow = Object.Instantiate(arrowPrefab, playerView.transform);
                    arrow.GetComponent<CharacterArrow>().SetCharacter(playerView);

                    _disposables.Add(player.Money.Subscribe(money =>
                    {
                        statView.SetMoney(money);
                    }));

                    _disposables.Add(
                        Observable.Merge(
                            player.Character.Inventory.CurrentItemCount.AsUnitObservable(),
                            player.Character.Inventory.Capacity.AsUnitObservable()
                        ).Subscribe(_ =>
                    {
                        var currentItems = player.Character.Inventory.CurrentItemCount.CurrentValue;
                        var capacity = player.Character.Inventory.Capacity.CurrentValue;
                        statView.SetInventory(currentItems, capacity);
                    }));

                    _disposables.Add(Observable
                        .Merge(player.Character.Status.HpValue, player.Character.Status.MaxHp)
                        .Subscribe(_ =>
                        {
                            var hpPercentageFromMaxHp = player.Character.CurrentHp * 100 /
                                                        player.Character.CurrentMaxHp;
                            statView.SetHp(player.Character.CurrentMaxHp,
                                player.Character.CurrentHp);
                            if (hpPercentageFromMaxHp < Settings.GlobalSettings.LowHpThresholdPercentage.CurrentValue)
                            {
                                statView.SetTextColor(Color.red);
                            }
                            else
                            {
                                statView.SetTextColor(Color.white);
                            }
                        }));
                },
                map => { _disposables.Clear(); });
        }
    }
}