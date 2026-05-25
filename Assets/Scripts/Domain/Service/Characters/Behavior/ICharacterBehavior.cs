using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;

namespace Domain.Service.Characters.Behavior
{
    public interface ICharacterBehavior : ISerializable<BehaviorMemento>, IItemSelector
    {
        public BehaviorData BehaviorData { get; }
        public Observable<OnStartItemSelectMessage> OnStartItemSelect { get; }
        public Observable<Unit> OnSelectedItemSelect { get; }

        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input);

        public void KnowLocationOf(Location location);
    }
}