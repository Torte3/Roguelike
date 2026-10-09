#nullable enable
using System;
using Domain.Model.Memento;
using Domain.Model.WorldEvents;
using R3;
using UnityEngine;
using Utilities;
using Utilities.Serialize.Option;

namespace Domain.Model.Entity
{
    public class EntityBase : IDisposable, ISerializable<EntityMemento>
    {
        public readonly Id<IEntity> Id;
        private readonly EntityLayer _layer;
        private readonly bool _ignoreGrass;
        private readonly ReactiveProperty<Vector2Int> _position;
        private readonly ReactiveProperty<bool> _isDestroyed;
        private IWorldEventRecorder? _events;
        private bool _isPlayer;
        private Func<Vector2Int, bool>? _isVisibleToPlayer;
        private Func<Vector2Int, Underfoot?>? _underfootAfterMove;

        public EntityBase(EntityMemento data, bool isVisualOnly = false)
        {
            Id = new Id<IEntity>(data.Id);
            _position = new(data.Position);
            _layer = data.Layer;
            _ignoreGrass = data.IgnoreGrass;
            _isDestroyed = new(data.IsDestroyed);
            IsVisualOnly = new(isVisualOnly);
        }

        public Vector2Int CurrentPosition => _position.CurrentValue;
        public ReadOnlyReactiveProperty<Vector2Int> Position => _position;
        public bool IsVisible { get; private set; }
        public EntityRef Ref => new(Id, CurrentPosition, IsVisible);
        public ReactiveProperty<bool> IsVisualOnly;
        public EntityLayer Layer => _layer;
        public bool IgnoreGrass => _ignoreGrass;
        public bool IsDestroyed => _isDestroyed.CurrentValue;
        public Observable<Unit> OnDestroyed => _isDestroyed.Where(isDestroyed => isDestroyed).AsUnitObservable();

        public void Dispose()
        {
            _position.Dispose();
        }

        public EntityMemento Serialize()
        {
            return new EntityMemento
            (
                Id.ToString(),
                CurrentPosition,
                _layer,
                IsDestroyed,
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
                false,
                ignoreGrass
            );
        }

        public EntityAppeared Appeared(EntityKind kind, Sprite? icon, bool isShiny = false)
        {
            return new EntityAppeared(Ref, AppearanceOf(kind, icon, isShiny));
        }

        public Appearance AppearanceOf(EntityKind kind, Sprite? icon, bool isShiny = false)
        {
            return new Appearance(kind, _layer, icon, isShiny);
        }

        public void EnterWorld(IWorldEventRecorder events, Func<Vector2Int, bool> isVisibleToPlayer, bool isPlayer,
            Func<Vector2Int, Underfoot?> underfootAfterMove)
        {
            _events = events;
            _isPlayer = isPlayer;
            _isVisibleToPlayer = isVisibleToPlayer;
            _underfootAfterMove = underfootAfterMove;
            IsVisible = isVisibleToPlayer(CurrentPosition);
        }

        public void LeaveWorld(Underfoot? underfoot)
        {
            Record(new EntityDisappeared(Ref, underfoot));
            _events = null;
            _isVisibleToPlayer = null;
            _underfootAfterMove = null;
        }

        public void Record(WorldEvent worldEvent)
        {
            _events?.Record(worldEvent);
        }

        public void SetVisibility(bool visible)
        {
            if (IsVisible == visible)
                return;
            IsVisible = visible;
            Record(new VisibilityChanged(Ref));
        }

        public void Move(Direction8 direction, MoveKind kind)
        {
            MoveTo(CurrentPosition + direction.Vector(), kind);
        }

        public void MoveTo(Vector2Int position, MoveKind kind)
        {
            if (_isVisibleToPlayer != null)
            {
                var wasVisible = IsVisible;
                IsVisible = _isVisibleToPlayer(position);
                Record(new EntityMoved(new EntityRef(Id, position, IsVisible), kind, wasVisible, _isPlayer,
                    _underfootAfterMove?.Invoke(position)));
            }

            _position.Value = position;
        }

        public void Teleport(Vector2Int position)
        {
            MoveTo(position, MoveKind.Teleport);
        }

        public void BlowTo(Vector2Int landing)
        {
            MoveTo(landing, MoveKind.Thrown);
        }

        public void Destroy()
        {
            _isDestroyed.Value = true;
        }
    }
}
