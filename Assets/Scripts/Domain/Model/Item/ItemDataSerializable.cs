#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Item
{
    [Serializable]
    public class ItemDataSerializable
    {
        [ShowIf(nameof(ShowItem))]
        [SerializeField] private ItemData _item;

        [ShowIf(nameof(ShowDirectWeapon))]
        [SerializeField] private DirectWeaponData _directWeapon;

        [ShowIf(nameof(ShowRangedWeapon))]
        [SerializeField] private RangedWeaponData _rangedWeapon;

        [ShowIf(nameof(ShowArtifact))]
        [SerializeField] private ArtifactData _artifact;

        private bool ShowItem => _directWeapon == null && _rangedWeapon == null && _artifact == null;
        private bool ShowDirectWeapon => _item == null && _rangedWeapon == null && _artifact == null;
        private bool ShowRangedWeapon => _item == null && _directWeapon == null && _artifact == null;
        private bool ShowArtifact => _item == null && _directWeapon == null && _rangedWeapon == null;

        public IItemData Value
        {
            get
            {
                if (_item != null) return _item;
                if (_directWeapon != null) return _directWeapon;
                if (_rangedWeapon != null) return _rangedWeapon;
                if (_artifact != null) return _artifact;
                throw new Exception("No item data is set");
            }
        }
    }
}
