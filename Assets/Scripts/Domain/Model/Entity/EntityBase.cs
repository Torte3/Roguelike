#nullable enable
using System;
using Cysharp.Threading.Tasks;
using Domain.Model.Memento;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Entity
{
    public abstract record EntityAppearance
    {
        public record Icon(int Id) : EntityAppearance;
        public record Prefab(string Name) : EntityAppearance;
    }
    public class EntityBase : IDisposable, ISerializable<EntityMemento>
    {
        public readonly Id<IEntity> Id;
        private readonly EntityLayer _layer;
        private readonly bool _ignoreGrass;
        private readonly Subject<(Direction8 direction, Vector2Int destination, bool isThrown)> _onMove = new();
        private readonly Subject<Vector2Int> _onTeleport = new();
        private readonly ReactiveProperty<Vector2Int> _position;
        private readonly ReactiveProperty<bool> _visibleByPlayer = new(false);
        private readonly ReactiveProperty<string?> _destroyLog;

        public EntityBase(EntityMemento data, bool isVisualOnly = false)
        {
            Id = new Id<IEntity>(data.Id);
            _position = new(data.Position);
            _layer = data.Layer;
            _ignoreGrass = data.IgnoreGrass;
            _destroyLog = new(data.DestroyLog.Value);
            IsVisualOnly = new(isVisualOnly);
        }

        public Vector2Int CurrentPosition => Position.CurrentValue;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _position;
        public Observable<(Direction8 direction, Vector2Int destination, bool isThrown)> OnMove => _onMove;
        public Observable<Vector2Int> OnTeleport => _onTeleport;
        public ReadOnlyReactiveProperty<bool> Visibility => _visibleByPlayer;
        public bool IsVisible => Visibility.CurrentValue;
        public ReactiveProperty<bool> IsVisualOnly;
        public EntityLayer Layer => _layer;
        public bool IgnoreGrass => _ignoreGrass;
        public bool IsDestroyed => _destroyLog.CurrentValue != null;
        public Observable<string> OnDestroyed => _destroyLog.WhereNotNull();
        public string? DestroyLog => _destroyLog.CurrentValue;

        public void Dispose()
        {
            _position.Dispose();
            _onMove.Dispose();
        }

        public EntityMemento Serialize()
        {
            return new EntityMemento
            (
                Id.ToString(),
                _position.CurrentValue,
                _layer,
                _destroyLog.CurrentValue.ToOption(),
                _ignoreGrass
            );
        }

        public static EntityMemento Build(Vector2Int position, EntityLayer layer, bool ignoreGrass = false)
        {
            return Build
            (
                Id<IEntity>.Generate(),
                position,
                layer,
                ignoreGrass
            );
        }

        public static EntityMemento Build(Id<IEntity> id, Vector2Int position, EntityLayer layer, bool ignoreGrass = false)
        {
            return new EntityMemento
            (
                id.ToString(),
                position,
                layer,
                Option<string>.None,
                ignoreGrass
            );
        }

        public void SetVisibility(bool visible)
        {
            _visibleByPlayer.Value = visible;
        }

        public void Teleport(Vector2Int position)
        {
            _position.Value = position;
            _onTeleport.OnNext(position);
        }

        public async UniTask Move(Direction8 direction, int moveMilliseconds, bool isThrown = false)
        {
            _position.Value += direction.Vector();
            _onMove.OnNext((direction, CurrentPosition, isThrown));
            if (Visibility.CurrentValue) await UniTask.Delay(moveMilliseconds);
        }

        public void Destroy(string destroyLog)
        {
            _destroyLog.Value = destroyLog;
        }
    }
}