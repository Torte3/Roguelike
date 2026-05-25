#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using ObservableCollections;
using R3;

namespace Utilities
{
    public static class ObservableExtension
    {
        public static Observable<T> SkipLatestValueOnSubscribe<T>(this ReadOnlyReactiveProperty<T> source)
        {
            return source.Skip(1);
        }

        public static IDisposable RelayTo<T>(this Observable<T> source, Observer<T> target)
        {
            return source.Subscribe(item => target.OnNext(item));
        }

        public static Observable<T> WhereNotNull<T>(this Observable<T?> source) where T : struct
        {
            return source.Where(item => item != null).Select(item => item.Value);
        }

        public static IDisposable SubscribeIncludingCurrentObservables<T, TMessage>(this IObservableCollection<T> list,
            Func<T, Observable<TMessage>> selector, Action<T, TMessage> action)
        {
            var disposables = new Dictionary<T, IDisposable>();
            var allDisposable = new CompositeDisposable();
            foreach (var item in list)
            {
                disposables[item] = selector(item).Subscribe(message => action(item, message));
                allDisposable.Add(disposables[item]);
            }

            list.ObserveAdd().Select(i => i.Value).Subscribe(item =>
            {
                disposables[item] = selector(item).Subscribe(message => action(item, message));
                allDisposable.Add(disposables[item]);
            });
            list.ObserveRemove().Select(i => i.Value).Subscribe(item =>
            {
                allDisposable.Remove(disposables[item]);
                disposables[item].Dispose();
            });
            list.ObserveReplace().Subscribe(value =>
            {
                allDisposable.Remove(disposables[value.OldValue]);
                disposables[value.OldValue].Dispose();
                disposables[value.NewValue] =
                    selector(value.NewValue).Subscribe(message => action(value.NewValue, message));
                allDisposable.Add(disposables[value.NewValue]);
            });
            return allDisposable;
        }

        public static IDisposable SubscribeIncludingCurrentItems<T>(this IObservableCollection<T> list, Action<T> addAction,
            Action<T>? removeAction = null)
        {
            foreach (var item in list)
            {
                addAction(item);
            }

            return new CompositeDisposable
            {
                list.ObserveAdd()
                    .Select(i => i.Value)
                    .Subscribe(addAction),
                list.ObserveRemove()
                    .Select(i => i.Value)
                    .Subscribe(i => removeAction?.Invoke(i)),
                list.ObserveReplace()
                    .Subscribe(value =>
                    {
                        removeAction?.Invoke(value.OldValue);
                        addAction(value.NewValue);
                    })
            };
        }

        public static IDisposable SubscribeIncludingCurrentValue<T>(this ReadOnlyReactiveProperty<T> property, Action<T> addAction,
            Action<T>? removeAction = null) where T : notnull
        {
            addAction(property.CurrentValue);

            return property.Pairwise()
                .Subscribe(value =>
                {
                    removeAction?.Invoke(value.Previous);
                    addAction(value.Current);
                });
        }

        public static IDisposable SubscribeIncludingCurrentValueIgnoreNull<T>(this ReadOnlyReactiveProperty<T?> property,
            Action<T> addAction,
            Action<T>? removeAction = null)
        {
            if (property.CurrentValue != null) addAction(property.CurrentValue);

            return property.Pairwise()
                .Subscribe(value =>
                {
                    if (value.Previous != null) removeAction?.Invoke(value.Previous);
                    if (value.Current != null) addAction(value.Current);
                });
        }

        public static void SynchronizeWith<T>(this ObservableHashSet<T> collectionA, IEnumerable<T> collectionB)
            where T : notnull
        {
            var itemsToRemove = collectionA.Except(collectionB).ToList();
            foreach (var item in itemsToRemove)
            {
                collectionA.Remove(item);
            }

            var itemsToAdd = collectionB.Except(collectionA).ToList();
            foreach (var item in itemsToAdd)
            {
                collectionA.Add(item);
            }
        }

        public static IDisposable LiveSynchronizeWith<T>(this ObservableHashSet<T> collectionA,
            IObservableCollection<T> collectionB) where T : notnull
        {
            collectionA.SynchronizeWith(collectionB);

            return collectionB.SubscribeIncludingCurrentItems(add => collectionA.Add(add), remove => collectionA.Remove(remove));
        }

        public static IDisposable AddWith<T, U>(this ObservableList<U> collectionA,
            IObservableCollection<T> collectionB) where T : notnull, U
        {
            return AddWith(collectionA, collectionB, x => x);
        }

        public static IDisposable AddWith<T, U>(this ObservableList<U> collectionA,
            IObservableCollection<T> collectionB, Func<T, U> selector) where T : notnull
        {
            return collectionB.SubscribeIncludingCurrentItems(
                add => collectionA.Add(selector(add)),
                remove => collectionA.Remove(selector(remove))
            );
        }

        public static IDisposable LiveSynchronizeWith<T>(this ICollection<T> collectionA,
            IObservableCollection<T> collectionB) where T : notnull
        {
            collectionA.SynchronizeWith(collectionB);

            return collectionB.SubscribeIncludingCurrentItems(add => collectionA.Add(add), remove => collectionA.Remove(remove));
        }
    }
}