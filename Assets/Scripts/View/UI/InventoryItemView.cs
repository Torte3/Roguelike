#nullable enable
using R3;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Utilities;

namespace View.UI
{
    [RequireComponent(typeof(Image), typeof(Selectable))]
    public class InventoryItemView : Selectable, ISelectHandler
    {
        public ItemViewData ItemData { get; private set; }
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _count;
        [SerializeField] private Image _cursedIcon;
        [SerializeField] private TMP_Text _name;
        private ParticleController _particles => _icon.GetComponent<ParticleController>();
        private readonly Subject<Unit> _onFocus = new();
        public Observable<Unit> OnSelected => _onFocus;

        public override void OnSelect(BaseEventData eventData)
        {
            _onFocus.OnNext(Unit.Default);
            base.OnSelect(eventData);
        }

        public void Set(ItemViewData itemData)
        {
            ItemData = itemData;
            SetIcon(itemData.icon);
            SetCount(itemData.count, itemData.showEquippedBadge, itemData.isCountIdentified);
            SetCursed(itemData.isCursed, itemData.isCurseIdentified);
            SetShiny(itemData.isShiny);
            SetName(itemData.name, itemData.isUsable);
        }

        private void SetIcon(Sprite? icon)
        {
            _icon.sprite = icon;
            _icon.enabled = icon != null;
        }

        private void SetCursed(bool isCursed, bool isIdentified)
        {
            if (!isIdentified)
            {
                _cursedIcon.enabled = false;
            }
            else
            {
                _cursedIcon.enabled = isCursed;
            }
        }

        private void SetShiny(bool isShiny)
        {
            if (isShiny)
                _particles.Add(ParticleType.ShinyStar);
            else
                _particles.Clear();
        }

        private void SetCount(int? count, bool showEquippedBadge, bool isIdentified)
        {
            if (showEquippedBadge)
            {
                _count.text = "E";
                return;
            }

            if (!isIdentified)
                _count.text = "?";
            else if (count.HasValue)
                _count.text = count.ToString();
            else
                _count.text = "";
        }

        private void SetName(string name, bool isUsable)
        {
            _name.text = name;
            _name.color = isUsable ? Colors.White : Colors.Gray;
        }

        public void UpdateInteractable(bool interactable)
        {
            this.interactable = interactable;
        }
    }
}