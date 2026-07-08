using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Result;

namespace Domain.Model.Effect
{
    public delegate UniTask<ISkillResult> SkillExecution();

    public interface ISkill
    {
        public bool IsDirectional { get; }
        public string Log => "";
        public float EvaluatePrice();
        public string Description();
        public EffectArea? AreaOf(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map, bool onlyVisible) => null;
    }
    public interface ISkillWithCost : ISerializable<SkillWithCostMemento>
    {
        public ISkill Skill { get; }
        public int Cost { get; }
        public string Log => Skill.Log;
        public void CoolDown();
        public bool IsUsable();
        public int ChargeTurn { get; }
        public int RushDistance { get; }
        public int BackStepDistance { get; }
        public UniTask<ISkillResult> Use(IActor actor, IItem? item, Vector2Int position, Direction8 direction, IMap map);
        public UniTask<Result<SkillExecution, Unit>> Prepare(IActor actor, IItem? item, Vector2Int position, Direction8 direction, IMap map);
        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map, IItem? sourceItem = null);
        public EffectArea? AreaOf(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map, bool onlyVisible);
        public float EvaluatePrice();
        public string Description();
    }
}