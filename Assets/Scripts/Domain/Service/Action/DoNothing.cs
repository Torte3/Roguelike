using Cysharp.Threading.Tasks;
using Domain.Model.Character;
using Domain.Model.Map;

namespace Domain.Service.Action
{
    public record DoNothing : IAction
    {
        public bool Doable(IActor actor, IMap map)
        {
            return true;
        }

        public UniTask Do(IActor actor, IMap map)
        {
            actor.DoNothing();
            return UniTask.CompletedTask;
        }

        public float Evaluate(IActor actor, IMap map)
        {
            return 0;
        }
    }
}