using System;
using Configuration;
using R3;
using UnityEngine;
using Utilities;
using View.Playback;

namespace View
{
    public class SEManager : MonoBehaviour
    {
        [SerializeField] private AudioSource _audioSource;
        [SerializeField] private AudioClip _grassWalkSE;
        [SerializeField] private AudioClip _attackSE;
        [SerializeField] private AudioClip _pickupSE;
        [SerializeField] private AudioClip _stairsSE;
        [SerializeField] private AudioClip _teleportSE;
        [SerializeField] private AudioClip _workbenchCraftSE;
        [SerializeField] private AudioClip _magicPotEnhanceSE;
        [SerializeField] private AudioClip _bonfireRestSE;
        [SerializeField] private AudioClip _choiceCursorSE;
        [SerializeField] private AudioClip _choiceConfirmSE;
        [SerializeField] private AudioClip _itemSelectCursorSE;
        [SerializeField] private AudioClip _itemSelectConfirmSE;
        [SerializeField] private AudioClip _openChestSE;
        [SerializeField] private AudioClip _shopCheckoutSE;
        [SerializeField] private AudioClip _trapStepSE;
        [SerializeField] private AudioClip _itemUsePotionSE;
        [SerializeField] private AudioClip _itemUseScrollSE;
        [SerializeField] private AudioClip _itemUseBookSE;
        [SerializeField] private AudioClip _itemUseWandSE;
        [SerializeField] private AudioClip _itemUseWeaponSE;
        [SerializeField] private AudioClip _itemUseOthersSE;
        [SerializeField] private AudioClip _equipSE;
        [SerializeField] private AudioClip _unequipSE;
        [SerializeField] private AudioClip _itemBreakSE;
        [SerializeField] private AudioClip _healSE;
        [SerializeField] private AudioClip _moneyPickupSE;
        [SerializeField] private AudioClip _chestUnlockSE;

        private void Awake()
        {
            Settings.GlobalSettings.SEVolume.Value
                .SubscribeIncludingCurrentValue(volume => _audioSource.volume = volume / 100f)
                .AddTo(this);
        }

        internal void Play(SeKind kind)
        {
            PlayOneShotIfNotNull(kind switch
            {
                SeKind.GrassWalk => _grassWalkSE,
                SeKind.Attack => _attackSE,
                SeKind.Heal => _healSE,
                SeKind.Pickup => _pickupSE,
                SeKind.MoneyPickup => _moneyPickupSE,
                SeKind.Stairs => _stairsSE,
                SeKind.Teleport => _teleportSE,
                SeKind.WorkbenchCraft => _workbenchCraftSE,
                SeKind.MagicPotEnhance => _magicPotEnhanceSE,
                SeKind.BonfireRest => _bonfireRestSE,
                SeKind.OpenChest => _openChestSE,
                SeKind.ChestUnlock => _chestUnlockSE,
                SeKind.ShopCheckout => _shopCheckoutSE,
                SeKind.TrapStep => _trapStepSE,
                SeKind.ItemUsePotion => _itemUsePotionSE,
                SeKind.ItemUseScroll => _itemUseScrollSE,
                SeKind.ItemUseBook => _itemUseBookSE,
                SeKind.ItemUseWand => _itemUseWandSE,
                SeKind.ItemUseWeapon => _itemUseWeaponSE,
                SeKind.ItemUseOthers => _itemUseOthersSE,
                SeKind.Equip => _equipSE,
                SeKind.Unequip => _unequipSE,
                SeKind.ItemBreak => _itemBreakSE,
                _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null),
            });
        }

        private void PlayOneShotIfNotNull(AudioClip? clip)
        {
            if (clip == null)
            {
                return;
            }

            _audioSource.PlayOneShot(clip);
        }

        internal void ChoiceCursorSE()
        {
            PlayOneShotIfNotNull(_choiceCursorSE);
        }

        public void ChoiceConfirmSE()
        {
            PlayOneShotIfNotNull(_choiceConfirmSE);
        }

        public void ItemSelectCursorSE()
        {
            PlayOneShotIfNotNull(_itemSelectCursorSE);
        }

        public void ItemSelectConfirmSE()
        {
            PlayOneShotIfNotNull(_itemSelectConfirmSE);
        }
    }
}
