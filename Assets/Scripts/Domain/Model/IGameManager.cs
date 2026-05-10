#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Character.Message;
using Domain.Model.Dungeon;
using Domain.Model.Entity;
using Domain.Model.Map;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model
{
    public interface IGameManager
    {
        public Observable<OnWorldIconPopupRequestedMessage> OnWorldIconPopupRequested { get; }
        public bool IsEventExecuting { get; }
        public Guid StartEvent();
        public void EndEvent(Guid eventId);
        public UniTask<int> GetChoice(string? text, params string[] choices);
        public UniTask<string> GetTextInput();
        public void MoveMap(Id<IMap> destination, Id<IEntity> from);
        public void PlayBGM(BGM bgm);
        public void PlaySE(SE se);
        public void PlayItemUseSE(ItemCategory category);
        public void RequestWorldIconPopup(Sprite icon, Vector2Int position);
        public void Save();
        public void SaveLight();
    }
}