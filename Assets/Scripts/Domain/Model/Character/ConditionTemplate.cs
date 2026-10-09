#nullable enable
using Domain.Model.Condition;
using Domain.Model.Effect;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
#endif

namespace Domain.Model.Character
{
    [CreateAssetMenu(fileName = "Condition", menuName = "ScriptableObject/ConditionTemplate")]
    public class ConditionTemplate : ScriptableObject
    {
        [Required] [SerializeReference] public IConditionData Condition;
        [Required] public RemovalConditionData RemovalCondition;
        public string InflictLog;
        public string DeleteLog;

        public float Evaluate(ITargetOfEffect target)
        {
            return Condition.Evaluate(target) * RemovalCondition.EvaluateTurn();
        }

        public float EvaluateDamage()
        {
            return Condition.EvaluatePrice() * RemovalCondition.EvaluateTurn();
        }
    }
}