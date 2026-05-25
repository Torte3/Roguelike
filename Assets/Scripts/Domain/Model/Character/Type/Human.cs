using System;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Character.Type
{
    [Serializable]
    public record Human : ICharacterType
    {
        [ShowInInspector] [OnValueChanged(nameof(OnValidate))]
        private Texture _texture;

        [ReadOnly] public string TextureName;

        public Human(string textureName)
        {
            TextureName = textureName;
        }

        private void OnValidate()
        {
            TextureName = _texture.name;
        }

        public string TypeName()
        {
            return "Human";
        }

        public string SubtypeName()
        {
            return TextureName;
        }
    }
}