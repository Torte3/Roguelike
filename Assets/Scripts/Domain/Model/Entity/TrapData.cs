using System;
using Domain.Model.Effect.Skill;
using UnityEngine;

namespace Domain.Model.Entity
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Trap")]
    public class TrapData : ScriptableObject
    {
        [field: SerializeField] public ActorlessSkillData Skill { get; private set; }

        [field: SerializeField]
        [field: Range(0, 1)]
        public float ProbabilityOfBreaking { get; private set; } = 0.1f;
    }
}