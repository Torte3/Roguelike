using System.Collections.Generic;
using System.Linq;
using ObservableCollections;

namespace Utilities
{
    public class IntegratedSet<T>
    {
        private readonly HashSet<IObservableCollection<T>> _collections = new();
        public readonly ObservableHashSet<T> _set = new();
        public IObservableCollection<T> Set => _set;

        public void Register(IObservableCollection<T> collection)
        {
            _collections.Add(collection);
            collection.SubscribeIncludingCurrentItems(Add, Remove);
        }

        public void UnRegister(IObservableCollection<T> collection)
        {
            _collections.Remove(collection);
            foreach (var item in collection)
            {
                Remove(item);
            }
        }

        public void Add(T item)
        {
            _set.Add(item);
        }

        public void Remove(T item)
        {
            if (!_collections.Any(collection => collection.Contains(item)))
            {
                _set.Remove(item);
            }
        }
    }
}