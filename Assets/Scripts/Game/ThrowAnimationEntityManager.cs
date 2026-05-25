#nullable enable
using Domain.Service.Items;
using ObservableCollections;
using Utilities;

namespace Game
{
    public class ThrowAnimationEntityManager
    {
        private readonly ObservableList<ThrowAnimationEntity> _throwAnimationEntities = new();

        public ThrowAnimationEntityManager()
        {
            _throwAnimationEntities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.OnDestroyed,
                (entity, destroyed) => Remove(entity)
            );
        }

        public IObservableCollection<ThrowAnimationEntity> ThrowAnimationEntities => _throwAnimationEntities;

        public void Add(ThrowAnimationEntity entity)
        {
            _throwAnimationEntities.Add(entity);
        }

        public void Remove(ThrowAnimationEntity entity)
        {
            _throwAnimationEntities.Remove(entity);
        }
    }
}