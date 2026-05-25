using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Domain.Model.Entity
{
    [CreateAssetMenu(fileName = "Data", menuName = "ScriptableObject/Statue")]
    public class StatueData : ScriptableObject
    {
        [field: SerializeField] public StatueType Type { get; private set; }
        [field: SerializeField] public ActorlessSkillData Skill { get; private set; }
        [field: SerializeField]
        [field: MinValue(1)]
        public int Cycle { get; private set; } = 10;

        [field: SerializeField]
        [field: MinValue(1)]
        public int AttackToBreak { get; private set; } = 3;
    }
}