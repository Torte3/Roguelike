#nullable enable
using Domain.Model.Character.Status;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Item;
using ObservableCollections;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Model.Character
{
    public interface IReadOnlyPlayerCharacter : IHasLabel, IItemKnowledge
    {
        public bool IsDead { get; }
        public Vector2Int Position { get; }
        public IReadOnlyStorage Inventory { get; }
        public IReadOnlyStatus Status { get; }
        public Observable<OnStartItemSelectMessage> OnStartItemSelect { get; }
        public Observable<Unit> OnSelectedItemSelect { get; }
        public EffectArea? PreviewEffectArea(ItemFocus focus);
        public bool IsVisible(Vector2Int position);
    }
}
