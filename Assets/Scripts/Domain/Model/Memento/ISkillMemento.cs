using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    public interface ISkillMemento
    {
    }
    [Serializable]
    public class SkillWithCostMemento
    {
        [field: SerializeReference] public ISkillMemento Skill;
        [field: SerializeField] public int Cost;
        [field: SerializeField] public int ChargeTurn;
        [field: SerializeField] public int CoolTime;
        [field: SerializeField] public int RemainingTurn;

        public SkillWithCostMemento(ISkillMemento skill, int cost, int chargeTurn, int coolTime, int remainingTurn)
        {
            Skill = skill;
            Cost = cost;
            ChargeTurn = chargeTurn;
            CoolTime = coolTime;
            RemainingTurn = remainingTurn;
        }
    }
}