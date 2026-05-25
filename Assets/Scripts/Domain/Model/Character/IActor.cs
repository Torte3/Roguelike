using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Item;
using Domain.Model.Map;
using Utilities;

namespace Domain.Model.Character
{
    public interface IActor : IActorOfEffect, IHasInventory
    {
        public Direction8 CurrentDirection { get; }
        public void DoNothing();
        public bool CanSwap(Direction8 direction, IMap map);
        public UniTask Move(Direction8 direction, IInput input);
        public void Turn(Direction8 direction);
        public void FaceNearestCharacter(IMap map);
        public UniTask UseSkill(ISkillWithCost skill, Direction8 direction, IMap map);
        public UniTask UseItem(IItem item, Direction8 direction, IMap map);
        public UniTask ThrowItem(IItem item, Direction8 direction, IMap map);
        public void DropItem(IItem item, IMap map);
        public void PickUpItem(IMap map);
        public float EvaluateThrow(IItem item, Direction8 direction, IMap map);
    }
}