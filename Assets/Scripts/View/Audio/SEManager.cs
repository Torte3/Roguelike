using System;
using Configuration;
using R3;
using UnityEngine;
using Utilities;
using View.Playback.Values;

namespace View.Audio
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

        internal void Play(SEKind kind)
        {
            PlayOneShotIfNotNull(kind switch
            {
                SEKind.GrassWalk => _grassWalkSE,
                SEKind.Attack => _attackSE,
                SEKind.Heal => _healSE,
                SEKind.Pickup => _pickupSE,
                SEKind.MoneyPickup => _moneyPickupSE,
                SEKind.Stairs => _stairsSE,
                SEKind.Teleport => _teleportSE,
                SEKind.WorkbenchCraft => _workbenchCraftSE,
                SEKind.MagicPotEnhance => _magicPotEnhanceSE,
                SEKind.BonfireRest => _bonfireRestSE,
                SEKind.OpenChest => _openChestSE,
                SEKind.ChestUnlock => _chestUnlockSE,
                SEKind.ShopCheckout => _shopCheckoutSE,
                SEKind.TrapStep => _trapStepSE,
                SEKind.ItemUsePotion => _itemUsePotionSE,
                SEKind.ItemUseScroll => _itemUseScrollSE,
                SEKind.ItemUseBook => _itemUseBookSE,
                SEKind.ItemUseWand => _itemUseWandSE,
                SEKind.ItemUseWeapon => _itemUseWeaponSE,
                SEKind.ItemUseOthers => _itemUseOthersSE,
                SEKind.Equip => _equipSE,
                SEKind.Unequip => _unequipSE,
                SEKind.ItemBreak => _itemBreakSE,
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
