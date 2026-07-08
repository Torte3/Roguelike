#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Entity;
using Domain.Model.Item;
using Domain.Model.Map;
using R3;
using Utilities;

namespace Domain.Model
{
    public interface IGameManager
    {
        public bool IsEventExecuting { get; }
        public Guid StartEvent();
        public void EndEvent(Guid eventId);
        public UniTask<int> GetChoice(ChoiceMessage? message, params string[] choices);
        public UniTask<int> GetChoice(ChoiceMessage? message, int cancelChoiceIndex, params string[] choices);
        public UniTask<int> GetChoiceWithItemPreview(ChoiceMessage? message, IMap map, params IItem[] items);
        public UniTask<int> GetChoiceWithItemPreview(ChoiceMessage? message, IMap map, int cancelChoiceIndex, params IItem[] items);
        public UniTask<string?> GetTextInput(bool canCancel = false);
        // 指定種類のチュートリアルを、未表示なら表示する（表示後に記録・保存）。
        public UniTask ShowTutorialIfNeeded(TutorialType type);
        public void MoveMap(Id<IMap> destination, Id<IEntity> from);
        public void Save();
        public void SaveLight();
    }
}