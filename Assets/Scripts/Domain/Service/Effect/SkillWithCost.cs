#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Result;

namespace Domain.Service.Effect
{
    public class SkillWithCost : ISkillWithCost
    {
        public ISkill Skill { get; }
        public int Cost { get; }
        public int RushDistance => Skill.Match(
            spawnEffectSkill => spawnEffectSkill.RushDistance,
            itemTargetSkill => 0,
            inventoryTargetSkill => 0,
            _ => 0
        );
        public int BackStepDistance => Skill.Match(
            spawnEffectSkill => spawnEffectSkill.BackStepDistance,
            itemTargetSkill => 0,
            inventoryTargetSkill => 0,
            _ => 0
        );
        public int CoolTime { get; private set; }
        private ReactiveProperty<int> _remainingCoolTime;
        public int ChargeTurn { get; private set; }
        public SkillWithCost(SkillWithCostMemento data)
        {
            Skill = data.Skill.Deserialize();
            Cost = data.Cost;
            ChargeTurn = data.ChargeTurn;
            CoolTime = data.CoolTime;
            _remainingCoolTime = new ReactiveProperty<int>(data.RemainingTurn);
        }

        public bool IsUsable() => _remainingCoolTime.CurrentValue <= 0;

        public SkillWithCostMemento Serialize()
        {
            return new SkillWithCostMemento(Skill.Serialize(), Cost, ChargeTurn, CoolTime, _remainingCoolTime.CurrentValue);
        }

        public static SkillWithCostMemento Build(ISkillMemento skill, int cost, int chargeTurn, int coolTime)
        {
            return new SkillWithCostMemento(skill, cost, chargeTurn, coolTime, 0);
        }

        public static SkillWithCostMemento Build(ISkillData skillData)
        {
            return Build(SpawnEffectSkill.Build(skillData), skillData.Cost, skillData.ChargeTurn, skillData.CoolTime);
        }

        public async UniTask<ISkillResult> Use(IActor actor, IItem item, Vector2Int position, Direction8 direction, IMap map)
        {
            var preparation = await Prepare(actor, item, position, direction, map);
            if (!preparation.IsOk(out var execution))
            {
                return SkillOutcome.NotRun(preparation);
            }

            return await execution();
        }

        public UniTask<Result<SkillExecution, Unit>> Prepare(IActor actor, IItem item, Vector2Int position, Direction8 direction,
            IMap map)
        {
            _remainingCoolTime.Value = CoolTime + 1;

            return Skill.Match(
                spawnEffectSkill => Prepared(() => spawnEffectSkill.Use(actor, item, position, direction, map)),
                itemTargetSkill => itemTargetSkill.Prepare(item, actor, map),
                inventoryTargetSkill => Prepared(() => inventoryTargetSkill.Use(actor.Inventory, actor, map)),
                equipToggleSkill => Prepared(() => equipToggleSkill.Use(actor, item!, position, direction, map))
            );
        }

        private static UniTask<Result<SkillExecution, Unit>> Prepared(SkillExecution execution)
        {
            return UniTask.FromResult(Result<SkillExecution, Unit>.Ok(execution));
        }

        public float Evaluate(IActorOfEffect actor, Vector2Int position, Direction8 direction, IMap map,
            IItem? sourceItem = null)
        {
            return Skill.Match(
                spawnEffectSkill => spawnEffectSkill.Evaluate(actor, sourceItem, position, direction, map),
                itemTargetSkill => itemTargetSkill.Evaluate(),
                inventoryTargetSkill => inventoryTargetSkill.Evaluate(),
                equipToggleSkill => equipToggleSkill.Evaluate(actor, position, direction, map)
            ) / (1 + ChargeTurn);
        }

        public float EvaluatePrice()
        {
            return Skill.EvaluatePrice() / (1 + ChargeTurn);
        }

        public EffectArea? AreaOf(IActorOfEffect actor, Vector2Int position,
            Direction8 direction, IMap map, bool onlyVisible)
        {
            return Skill.AreaOf(actor, position, direction, map, onlyVisible);
        }

        public void CoolDown()
        {
            if (_remainingCoolTime.CurrentValue > 0)
            {
                _remainingCoolTime.Value--;
            }
        }

        public string Description()
        {
            var description = Skill.Description();
            if (ChargeTurn > 0)
                description += $"発動には{ItemDescriptionRichText.RichTurns(ChargeTurn + 1)}ターンかかる\n";
            if (CoolTime > 0)
                description += $"発動後に{ItemDescriptionRichText.RichTurns(CoolTime)}ターンは再使用不能\n";
            return description;
        }
    }
}