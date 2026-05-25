#nullable enable
using System;
using UnityEngine;

namespace Domain.Model.Memento
{
    [Serializable]
    public class CharacterSkillMemento
    {
        [field: SerializeField] public SpawnEffectSkillMemento Skill { get; private set; }
        [field: SerializeField] public int RushDistance { get; private set; }
        [field: SerializeField] public int BackStepDistance { get; private set; }
        [field: SerializeField] public int ChargeTurn { get; private set; }
        [field: SerializeField] public int CoolTime { get; private set; }
        [field: SerializeField] public int RemainingTurn { get; private set; }

        public CharacterSkillMemento(
            SpawnEffectSkillMemento skill,
            int rushDistance,
            int backStepDistance,
            int chargeTurn,
            int coolTime,
            int remainingTurn)
        {
            Skill = skill;
            RushDistance = rushDistance;
            BackStepDistance = backStepDistance;
            ChargeTurn = chargeTurn;
            CoolTime = coolTime;
            RemainingTurn = remainingTurn;
        }
    }
}