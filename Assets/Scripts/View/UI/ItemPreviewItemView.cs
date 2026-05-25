#nullable enable
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Utilities;

namespace View.UI
{
    public class ItemPreviewItemView : MonoBehaviour
    {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _count;
        [SerializeField] private Image _cursedIcon;
        [SerializeField] private TMP_Text _name;

        private ParticleController? _particles;

        private void Awake()
        {
            _particles = _icon.GetComponent<ParticleController>();
        }

        public void Set(ItemPreviewViewData data)
        {
            SetIcon(data.icon);
            SetCount(data.count, data.showEquippedBadge, data.isCountIdentified);
            SetCursed(data.isCursed, data.isCurseIdentified);
            SetShiny(data.isShiny);
            SetName(data.name);
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
            if (_particles == null)
                return;
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

        private void SetName(string name)
        {
            _name.text = name;
            _name.color = Colors.White;
        }
    }
}
