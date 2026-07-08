using System.Text;
using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Map;
using Unity.Logging;
using Utilities;

namespace Domain.Service.Action
{
    internal record UseSkill(ISkillWithCost Skill, Direction8 Direction) : IAction
    {
        protected virtual bool PrintMembers(StringBuilder builder)
        {
            builder.Append($"Skill = {Skill.Description()}, Direction = {Direction}");
            return true;
        }

        public bool Doable(IActor actor, IMap map)
        {
            if (actor.Status.IsFlagStat(FlagStatType.CannotAct))
            {
                Log.Debug($"[InputBlock][UseSkill] reason:Actor has CannotAct flag., skill:{Skill.Description()}, direction:{Direction}");
                return false;
            }

            if (!Skill.IsUsable())
            {
                Log.Debug($"[InputBlock][UseSkill] reason:Skill.IsUsable() is false., skill:{Skill.Description()}, direction:{Direction}");
                return false;
            }

            return true;
        }

        public async UniTask Do(IActor actor, IMap map)
        {
            await actor.UseSkill(Skill, Direction, map);
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return Skill.Evaluate(actor, actor.Entity.CurrentPosition, Direction, map);
        }
    }
}