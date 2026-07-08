using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Result;

namespace Domain.Model.Effect.Skill
{
    public delegate UniTask<ISkillResult> SkillExecution();

    public interface ISkill
    {
        public bool IsDirectional { get; }
        public string Log => "";
        public float EvaluatePrice();
        public string Description();
    }
    public interface ISkillWithCost : ISerializable<SkillWithCostMemento>, IReadOnlySkill
    {
        public ISkill Skill { get; }
        public int Cost { get; }
        public string Log => Skill.Log;
        public void CoolDown();
        public bool IsUsable();
        public int ChargeTurn { get; }
        public int RushDistance { get; }
        public int BackStepDistance { get; }
        public UniTask<ISkillResult> Use(IActor actor, IItem? item, Vector2Int position, Direction8 direction, IMap map,
            SkillSource source);
        public UniTask<Result<SkillExecution, Unit>> Prepare(IActor actor, IItem? item, Vector2Int position, Direction8 direction, IMap map,
            SkillSource source);
        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map, IItem? sourceItem = null);
        public float EvaluatePrice();
    }
}