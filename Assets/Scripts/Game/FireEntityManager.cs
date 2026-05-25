#nullable enable
using System.Collections.Generic;
using System.Linq;
using Domain.Model;
using Domain.Model.Evaluation;
using Domain.Model.Map;
using Domain.Model.Memento;
using Domain.Service.Events;
using ObservableCollections;
using UnityEngine;
using Utilities;

namespace Game
{
    public class FireEntityManager
    {
        private readonly ObservableList<Fire> _fireEntities = new();

        public FireEntityManager(FireEntitiesMemento memento)
        {
            foreach (var fireMemento in memento.Fires)
            {
                Add(new Fire(fireMemento));
            }

            _fireEntities.SubscribeIncludingCurrentObservables(
                entity => entity.Entity.OnDestroyed,
                (entity, destroyed) => Remove(entity)
            );
        }

        public FireEntitiesMemento Serialize()
        {
            return new FireEntitiesMemento(_fireEntities.Select(fire => fire.Serialize()).ToList());
        }

        public static FireEntitiesMemento Build()
        {
            return new FireEntitiesMemento(new List<EntityMemento>());
        }

        public IObservableCollection<Fire> FireEntities => _fireEntities;

        public void Add(Fire entity)
        {
            if (!_fireEntities.Any(fire => entity.Entity.CurrentPosition == fire.Entity.CurrentPosition))
            {
                _fireEntities.Add(entity);
            }
        }

        public void Remove(Fire entity)
        {
            _fireEntities.Remove(entity);
        }

        public void UpdateTurn(IMap map)
        {
            var destroyedFires = new List<Fire>();
            var addedFires = new List<Fire>();
            foreach (var fire in _fireEntities)
            {
                if (RandUtils.IsLessThanProbability(CommonSenseParameters.DestroyFireProbabilityPerTurn))
                {
                    destroyedFires.Add(fire);
                }

                var positions = DirectionMethods
                    .AllDirections
                    .Select(direction => fire.Entity.CurrentPosition + direction.Vector())
                    .Where(position => map.At(position).CanPlace(false, false, true));
                foreach (var position in positions)
                {
                    if (RandUtils.IsLessThanProbability(GetProbabilityOfFireSpreading(position, map)))
                    {
                        addedFires.Add(new Fire(Fire.Build(position)));
                    }
                }
            }

            foreach (var fire in destroyedFires)
            {
                fire.Entity.Destroy("は自然に消えた");
            }

            foreach (var fire in addedFires)
            {
                Add(fire);
            }
        }

        public float GetProbabilityOfFireSpreading(Vector2Int position, IMap map)
        {
            var value = 1 / 64f;
            if (map.IsGrass(position))
                value += 1 / 8f;
            if (map.Entities.At(position).Any())
                value += 1 / 16f;
            return value;
        }
    }
}