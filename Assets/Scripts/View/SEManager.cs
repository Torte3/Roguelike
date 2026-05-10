using Domain.Model.Dungeon;
using UnityEngine;

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

        public void SetVolume(float volume)
        {
            _audioSource.volume = volume;
        }

        private void PlayOneShotIfNotNull(AudioClip? clip)
        {
            if (clip == null)
            {
                return;
            }

            _audioSource.PlayOneShot(clip);
        }

        public void GrassWalkSE()
        {
            PlayOneShotIfNotNull(_grassWalkSE);
        }

        public void AttackSE()
        {
            PlayOneShotIfNotNull(_attackSE);
        }

        public void PickupSE()
        {
            PlayOneShotIfNotNull(_pickupSE);
        }

        public void StairsSE()
        {
            PlayOneShotIfNotNull(_stairsSE);
        }

        public void TeleportSE()
        {
            PlayOneShotIfNotNull(_teleportSE);
        }

        public void WorkbenchCraftSE()
        {
            PlayOneShotIfNotNull(_workbenchCraftSE);
        }

        public void MagicPotEnhanceSE()
        {
            PlayOneShotIfNotNull(_magicPotEnhanceSE);
        }

        public void BonfireRestSE()
        {
            PlayOneShotIfNotNull(_bonfireRestSE);
        }

        public void ChoiceCursorSE()
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

        public void OpenChestSE()
        {
            PlayOneShotIfNotNull(_openChestSE);
        }

        public void ShopCheckoutSE()
        {
            PlayOneShotIfNotNull(_shopCheckoutSE);
        }

        public void TrapStepSE()
        {
            PlayOneShotIfNotNull(_trapStepSE);
        }

        public void ItemUseSE(ItemCategory category)
        {
            PlayOneShotIfNotNull(category switch
            {
                ItemCategory.Potions => _itemUsePotionSE,
                ItemCategory.Scrolls => _itemUseScrollSE,
                ItemCategory.Books => _itemUseBookSE,
                ItemCategory.Wands => _itemUseWandSE,
                ItemCategory.Weapons => _itemUseWeaponSE,
                ItemCategory.Artifacts => null,
                ItemCategory.Others => _itemUseOthersSE,
                _ => null
            });
        }
    }
}
