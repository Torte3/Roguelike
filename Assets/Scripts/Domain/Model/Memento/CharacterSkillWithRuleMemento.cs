#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterSkillWithRuleMemento
    {
        [field: SerializeField] public SkillWithCostMemento Skill;
        [field: SerializeField] public int Priority;

        public CharacterSkillWithRuleMemento(SkillWithCostMemento skill, int priority)
        {
            Skill = skill;
            Priority = priority;
        }
    }
}