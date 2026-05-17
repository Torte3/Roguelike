using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using UnityEngine;
using Utilities;

namespace Domain.Model.Effect
{
    public interface ISkill
    {
        public bool IsDirectional { get; }
        public float EvaluatePrice();
    }
    public interface ISkillWithCost : ISerializable<SkillWithCostMemento>, IHasInfo
    {
        public ISkill Skill { get; }
        public int Cost { get; }
        public void CoolDown();
        public bool IsUsable();
        public int ChargeTurn { get; }
        public int RushDistance { get; }
        public int BackStepDistance { get; }
        public UniTask<ISkillResult> Use(IActor actor, IItem? item, Vector2Int position, Direction8 direction, IMap map);
        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map, IItem? sourceItem = null);
        public float EvaluatePrice();
    }
}