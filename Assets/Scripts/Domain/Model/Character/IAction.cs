using Cysharp.Threading.Tasks;
using Domain.Model.Map;

namespace Domain.Model.Character
{
    public interface IAction
    {
        public bool Doable(IActor actor, IMap map);
        public UniTask Do(IActor actor, IMap map);

        /// <summary>
        ///     Calculates the expected profit for the Actor when doing the action.
        ///     It doesn't care if it's doable or not.
        /// </summary>
        /// <param name="actor">The actor of this action.</param>
        /// <returns></returns>
        public float Evaluate(IActor actor, IMap map);
    }
}