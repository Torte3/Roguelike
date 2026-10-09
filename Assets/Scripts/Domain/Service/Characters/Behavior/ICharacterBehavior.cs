using Cysharp.Threading.Tasks;
using Domain.Model;
using Domain.Model.Character;
using Domain.Model.Dungeon;
using Domain.Model.Item;
using Domain.Model.Map;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

#nullable enable

namespace Domain.Service.Characters.Behavior
{
    public interface ICharacterBehavior : ISerializable<BehaviorMemento>
    {
        public BehaviorData BehaviorData { get; }
        public Observable<OnStartItemSelectMessage> OnStartItemSelect { get; }
        public Observable<Unit> OnSelectedItemSelect { get; }

        public UniTask<IAction> GenerateNextAction(IHasBehavior character, IGameManager gameManager, IMap map,
            IInput input);

        public void KnowLocationOf(Location location);

        public bool AcceptsSwapFrom(IHasBehavior character, Vector2Int requesterPosition, IMap map);

        // 旧 IItemSelector。合成元がこのIFのみで独立利用が無かったため統合した。
        public UniTask<ItemFocus> SelectItem(string text, ItemFocus[] disabledItems);
        public UniTask<ItemFocus> SelectItemWithPreview(string text, ItemFocus[] disabledItems,
            ItemSelectPreview[] previews, ItemSelectPreview? defaultPreview, string previewTitle);
    }
}