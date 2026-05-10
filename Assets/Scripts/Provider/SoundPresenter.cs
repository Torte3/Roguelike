using System;
using Domain.Model;
using Domain.Model.Setting;
using Game;
using R3;
using Utilities;
using VContainer;
using View;

namespace Provider
{
    public class SoundPresenter
    {
        private readonly CompositeDisposable _disposable = new();

        [Inject]
        public SoundPresenter(GameManager gameManager, World world, BGMManager bgmManager, SEManager seManager)
        {
            Settings.GlobalSettings.BGMVolume.Value.SubscribeIncludingCurrentValue(volume => bgmManager.SetVolume(volume / 100f));
            Settings.GlobalSettings.SEVolume.Value.SubscribeIncludingCurrentValue(volume => seManager.SetVolume(volume / 100f));
            world.OnActiveMapChanged.Subscribe(mapChanged =>
                {
                    var map = mapChanged.Map;
                    _disposable.Add(map.OnEffectSpawned.Subscribe(effectSpawned => { seManager.AttackSE(); }));
                    _disposable.Add(map.Player.Character.Entity.OnTeleport.Subscribe(teleport => { seManager.TeleportSE(); }));
                },
                _ => _disposable.Clear());
            gameManager.OnPlayBGM.Subscribe(bgm =>
            {
                switch (bgm)
                {
                    case BGM.Normal:
                        bgmManager.NormalBGM();
                        break;
                    case BGM.Stolen:
                        bgmManager.StolenBGM();
                        break;
                    case BGM.Shop:
                        bgmManager.ShopBGM();
                        break;
                    case BGM.MonsterHouse:
                        bgmManager.MonsterHouseBGM();
                        break;
                    default:
                        break;
                }
            });
            gameManager.OnPlaySE.Subscribe(se =>
            {
                switch (se)
                {
                    case SE.GrassWalk:
                        seManager.GrassWalkSE();
                        break;
                    case SE.Attack:
                        seManager.AttackSE();
                        break;
                    case SE.Pickup:
                        seManager.PickupSE();
                        break;
                    case SE.Stairs:
                        seManager.StairsSE();
                        break;
                    case SE.Teleport:
                        seManager.TeleportSE();
                        break;
                    case SE.WorkbenchCraft:
                        seManager.WorkbenchCraftSE();
                        break;
                    case SE.MagicPotEnhance:
                        seManager.MagicPotEnhanceSE();
                        break;
                    case SE.BonfireRest:
                        seManager.BonfireRestSE();
                        break;
                    case SE.ChoiceCursor:
                        seManager.ChoiceCursorSE();
                        break;
                    case SE.ChoiceConfirm:
                        seManager.ChoiceConfirmSE();
                        break;
                    case SE.ItemSelectCursor:
                        seManager.ItemSelectCursorSE();
                        break;
                    case SE.ItemSelectConfirm:
                        seManager.ItemSelectConfirmSE();
                        break;
                    case SE.OpenChest:
                        seManager.OpenChestSE();
                        break;
                    case SE.ShopCheckout:
                        seManager.ShopCheckoutSE();
                        break;
                    case SE.TrapStep:
                        seManager.TrapStepSE();
                        break;
                    default:
                        throw new NotImplementedException($"SE {se} is not implemented");
                }
            });
            gameManager.OnPlayItemUseSE.Subscribe(seManager.ItemUseSE);
        }
    }
}