#nullable enable
using Cysharp.Threading.Tasks;
using Domain.Model.Effect;
using Domain.Model.Entity;
using Domain.Model.Map;
using Domain.Model.Setting;
using R3;
using UnityEngine;
using Utilities;

namespace Domain.Service.Items
{
    public class ThrowAnimationEntity : IEntity
    {
        public EntityBase Entity { get; init; }
        public bool IsGrounded => false;
        private readonly ReactiveProperty<bool> _isVisualOnly = new(true);
        public ReadOnlyReactiveProperty<bool> IsVisualOnly => _isVisualOnly;
        public readonly Sprite Icon;

        public ThrowAnimationEntity(Vector2Int position, Sprite icon)
        {
            Entity = new EntityBase(EntityBase.Build(position, EntityLayer.Middle), true);
            Icon = icon;
        }

        public async UniTask<Vector2Int> Throw(Direction8 direction, IMap map, int distance, bool isPiercing,
            params EntityLayer[] canHitLayer)
        {
            for (var i = 0; i < distance; i++)
            {
                if (map.At(Entity.CurrentPosition + direction.Vector()).IsBlank(canHitLayer))
                {
                    await Entity.Move(direction, Settings.GlobalSettings.ThrowMilliseconds.CurrentValue, true);
                }
                else if (map.At(Entity.CurrentPosition + direction.Vector()).IsPassableOnMap())
                {
                    await Entity.Move(direction, Settings.GlobalSettings.ThrowMilliseconds.CurrentValue, true);
                    if (!isPiercing)
                        break;
                }
                else
                {
                    break;
                }
            }

            return Entity.CurrentPosition;
        }

        public void Dispose()
        {
            Entity.Dispose();
        }

        public UniTask BlowAway(IActorOfEffect actor, Direction8 direction, int distance, IMap map)
        {
            return UniTask.CompletedTask;
        }
    }
}