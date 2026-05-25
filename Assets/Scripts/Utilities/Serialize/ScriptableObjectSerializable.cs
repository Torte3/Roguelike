#nullable enable
using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Utilities.Serialize
{
    [Serializable]
    public class ScriptableObjectSerializable<T> where T : ScriptableObject
    {
        [ShowInInspector]
        [OnValueChanged(nameof(OnValidate))]
        [OnInspectorInit(nameof(OnInspectorInit))]
        private T _value;

        [HideInInspector]
        [SerializeField]
        private string _name;

        public T Value
        {
            get
            {
                if (_value == null)
                {
                    _value = ObjectLoader.Load<T>(_name);
                }

                return _value;
            }
        }

        public ScriptableObjectSerializable(T value)
        {
            _value = value;
            _name = value.name;
        }

        private void OnValidate()
        {
            _name = _value.name;
        }

        private void OnInspectorInit()
        {
            if (_value == null)
            {
                _value = ObjectLoader.Load<T>(_name);
            }
        }
    }
}