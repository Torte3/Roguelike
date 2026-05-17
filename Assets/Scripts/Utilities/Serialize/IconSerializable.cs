#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utilities.Serialize
{
    [Serializable]
    public class IconSerializable
    {
        [ShowInInspector]
#if UNITY_EDITOR
        [OnValueChanged(nameof(OnValidate))]
#endif
        private Sprite _value;

        [Required]
        [ReadOnly]
        [SerializeField]
        private string _name;

        public Sprite Value
        {
            get
            {
                if (_value == null)
                {
                    _value = ObjectLoader.LoadIcon(_name);
                }

                return _value;
            }
        }

        public IconSerializable(Sprite value)
        {
            _value = value;
            _name = value.name;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            _name = _value.name;
        }
#endif
    }
}