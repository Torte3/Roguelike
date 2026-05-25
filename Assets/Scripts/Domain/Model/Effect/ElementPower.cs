using System;
using System.Collections.Generic;
using Domain.Model.Item;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Effect
{
    [Serializable]
    public class ElementPower : IHasInfo
    {
        [SerializeField] private Element _element;
        [MinValue(1)] [SerializeField] private int _power;
        public Element Element => _element;
        public int Power => _power;

        public ElementPower(Element element, int power)
        {
            _element = element;
            _power = power;
        }

        public ElementPower MultiplyPower(float multiplier)
        {
            return new ElementPower(Element, Mathf.RoundToInt(_power * multiplier));
        }

        public string Info()
        {
            return $"{Element.Name()}属性、威力{Power}";
        }
    }
}